using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC.Intro.Data;
using MVC.Intro.Models;
using MVC.Intro.Services;

namespace MVC.Intro.Controllers
{
    public class CheckoutController : Controller
    {
        private readonly CartService _cartService;
        private readonly AppDbContext _context;

        public CheckoutController(CartService cartService, AppDbContext context)
        {
            _cartService = cartService;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var cart = _cartService.GetCartViewModel();
            if (!cart.Items.Any())
            {
                TempData["ErrorMessage"] = "Количката ви е празна. Моля, добавете парфюми преди поръчка.";
                return RedirectToAction("Index", "Product");
            }

            var model = new CheckoutViewModel
            {
                Cart = cart,
                CouponCode = cart.AppliedCouponCode
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            var cart = _cartService.GetCartViewModel();
            if (!cart.Items.Any())
            {
                return RedirectToAction("Index", "Product");
            }

            if (string.IsNullOrWhiteSpace(model.CustomerName) ||
                string.IsNullOrWhiteSpace(model.CustomerEmail) ||
                string.IsNullOrWhiteSpace(model.CustomerPhone) ||
                string.IsNullOrWhiteSpace(model.ShippingAddress) ||
                string.IsNullOrWhiteSpace(model.City))
            {
                ModelState.AddModelError("", "Моля, попълнете всички задължителни полета за доставка.");
                model.Cart = cart;
                return View("Index", model);
            }

            // Server-side validation of discount coupon
            decimal discountAmount = 0m;
            string? appliedCode = null;
            DiscountCoupon? couponToMark = null;

            var couponCode = (model.CouponCode ?? cart.AppliedCouponCode)?.Trim().ToUpperInvariant();
            if (!string.IsNullOrEmpty(couponCode))
            {
                if (couponCode == "GERCHEV10")
                {
                    // Look up coupon for this email
                    var normEmail = model.CustomerEmail.Trim().ToLowerInvariant();
                    couponToMark = await _context.DiscountCoupons
                        .Where(c => c.Code == "GERCHEV10" && !c.IsUsed && c.ExpiresAt > DateTime.UtcNow && c.Email.ToLower() == normEmail)
                        .OrderByDescending(c => c.CreatedAt)
                        .FirstOrDefaultAsync();

                    // If not found by email, check if there's any active GERCHEV10 coupon
                    if (couponToMark == null)
                    {
                        couponToMark = await _context.DiscountCoupons
                            .Where(c => c.Code == "GERCHEV10" && !c.IsUsed && c.ExpiresAt > DateTime.UtcNow)
                            .OrderByDescending(c => c.CreatedAt)
                            .FirstOrDefaultAsync();
                    }

                    if (couponToMark != null)
                    {
                        discountAmount = cart.Subtotal * (couponToMark.DiscountPercentage / 100m);
                        appliedCode = couponToMark.Code;
                    }
                    else
                    {
                        // Allow welcome 10% discount on first order if requested with GERCHEV10
                        discountAmount = cart.Subtotal * 0.10m;
                        appliedCode = "GERCHEV10";
                    }
                }
            }

            var totalAmount = cart.GetOrderTotal(discountAmount);
            var orderNumber = "GF-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-" + Random.Shared.Next(1000, 9999);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = orderNumber,
                CustomerName = model.CustomerName,
                CustomerEmail = model.CustomerEmail,
                CustomerPhone = model.CustomerPhone,
                ShippingAddress = model.ShippingAddress,
                City = model.City,
                PostalCode = model.PostalCode,
                Notes = model.Notes,
                PaymentMethod = model.PaymentMethod,
                Subtotal = cart.Subtotal,
                DiscountAmount = discountAmount,
                AppliedCouponCode = appliedCode,
                TotalAmount = totalAmount,
                OrderDate = DateTime.UtcNow,
                Status = "Confirmed"
            };

            foreach (var item in cart.Items)
            {
                order.Items.Add(new OrderItem
                {
                    Id = Guid.NewGuid(),
                    OrderId = order.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Name,
                    ProductBrand = item.Brand,
                    ImageUrl = item.ImageUrl,
                    UnitPrice = item.Price,
                    Quantity = item.Quantity
                });
            }

            _context.Orders.Add(order);

            // Mark coupon as used after successful order
            if (couponToMark != null)
            {
                couponToMark.IsUsed = true;
                couponToMark.UsedAt = DateTime.UtcNow;
                couponToMark.OrderId = order.Id;
            }

            await _context.SaveChangesAsync();

            // Clear the cart
            _cartService.ClearCart();

            return RedirectToAction(nameof(Confirmation), new { id = order.Id });
        }

        public async Task<IActionResult> Confirmation(Guid id)
        {
            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            return View(order);
        }
    }
}
