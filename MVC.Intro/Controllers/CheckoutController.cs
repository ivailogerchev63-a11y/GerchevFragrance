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
        private readonly IEmailService _emailService;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(CartService cartService, AppDbContext context,
            IEmailService emailService, ILogger<CheckoutController> logger)
        {
            _cartService = cartService;
            _context = context;
            _emailService = emailService;
            _logger = logger;
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

            // Pre-fill customer info from authenticated user
            var model = new CheckoutViewModel
            {
                Cart = cart,
                CouponCode = cart.AppliedCouponCode
            };

            if (User.Identity?.IsAuthenticated == true)
            {
                model.CustomerEmail = User.Identity.Name ?? string.Empty;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutViewModel model)
        {
            // Always re-load cart from server session — never trust client-submitted cart data
            var cart = _cartService.GetCartViewModel();
            if (!cart.Items.Any())
                return RedirectToAction("Index", "Product");

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

            // Server-side discount validation
            decimal discountAmount = 0m;
            string? appliedCode = null;
            DiscountCoupon? couponToMark = null;

            var couponCode = (model.CouponCode ?? cart.AppliedCouponCode)?.Trim().ToUpperInvariant();
            if (!string.IsNullOrEmpty(couponCode))
            {
                if (couponCode == "GERCHEV10")
                {
                    var normEmail = model.CustomerEmail.Trim().ToLowerInvariant();
                    couponToMark = await _context.DiscountCoupons
                        .Where(c => c.Code == "GERCHEV10" && !c.IsUsed && c.ExpiresAt > DateTime.UtcNow && c.Email.ToLower() == normEmail)
                        .OrderByDescending(c => c.CreatedAt)
                        .FirstOrDefaultAsync();

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
                        // Apply standard 10% welcome discount
                        discountAmount = cart.Subtotal * 0.10m;
                        appliedCode = "GERCHEV10";
                    }
                }
            }

            var shippingFee = cart.ShippingFee;
            var totalAmount = Math.Max(0, cart.Subtotal - discountAmount) + shippingFee;
            var orderNumber = "GF-" + DateTime.UtcNow.ToString("yyyyMMdd") + "-" + Random.Shared.Next(1000, 9999);

            var order = new Order
            {
                Id = Guid.NewGuid(),
                OrderNumber = orderNumber,
                CustomerName = model.CustomerName.Trim(),
                CustomerEmail = model.CustomerEmail.Trim(),
                CustomerPhone = model.CustomerPhone.Trim(),
                ShippingAddress = model.ShippingAddress.Trim(),
                City = model.City.Trim(),
                PostalCode = model.PostalCode?.Trim(),
                Notes = model.Notes?.Trim(),
                PaymentMethod = model.PaymentMethod,
                Subtotal = cart.Subtotal,
                DiscountAmount = discountAmount,
                AppliedCouponCode = appliedCode,
                ShippingFee = shippingFee,
                TotalAmount = totalAmount,
                OrderDate = DateTime.UtcNow,
                Status = "Потвърдена"
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
                    SelectedSize = item.Size,
                    UnitPrice = item.Price,
                    Quantity = item.Quantity
                });
            }

            _context.Orders.Add(order);

            if (couponToMark != null)
            {
                couponToMark.IsUsed = true;
                couponToMark.UsedAt = DateTime.UtcNow;
                couponToMark.OrderId = order.Id;
            }

            await _context.SaveChangesAsync();

            // Clear cart BEFORE sending email to prevent double-clear issues
            _cartService.ClearCart();

            // Send confirmation email (non-blocking - failure does not affect order)
            var emailSent = await _emailService.SendOrderConfirmationAsync(order);
            if (emailSent)
            {
                order.ConfirmationEmailSent = true;
                await _context.SaveChangesAsync();
            }

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
