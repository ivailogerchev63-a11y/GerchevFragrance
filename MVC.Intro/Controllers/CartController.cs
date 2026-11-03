using Microsoft.AspNetCore.Mvc;
using MVC.Intro.Services;

namespace MVC.Intro.Controllers
{
    public class CartController : Controller
    {
        private readonly CartService _cartService;

        public CartController(CartService cartService)
        {
            _cartService = cartService;
        }

        public IActionResult Index()
        {
            var cart = _cartService.GetCartViewModel();
            return View(cart);
        }

        [HttpPost]
        public async Task<IActionResult> Add(Guid id, int quantity = 1)
        {
            var success = await _cartService.AddToCartAsync(id, quantity);
            var cart = _cartService.GetCartViewModel();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers.Accept.ToString().Contains("application/json"))
            {
                return Json(new
                {
                    success,
                    totalItems = cart.TotalItems,
                    subtotal = cart.Subtotal.ToString("F2"),
                    total = cart.Total.ToString("F2"),
                    message = success ? "Продуктът беше добавен в количката!" : "Възникна грешка при добавянето."
                });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult UpdateQuantity(Guid id, int quantity)
        {
            _cartService.UpdateQuantity(id, quantity);
            var cart = _cartService.GetCartViewModel();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    totalItems = cart.TotalItems,
                    subtotal = cart.Subtotal.ToString("F2"),
                    discount = cart.DiscountAmount.ToString("F2"),
                    total = cart.Total.ToString("F2")
                });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Remove(Guid id)
        {
            _cartService.RemoveFromCart(id);
            var cart = _cartService.GetCartViewModel();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    totalItems = cart.TotalItems,
                    subtotal = cart.Subtotal.ToString("F2"),
                    total = cart.Total.ToString("F2")
                });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public async Task<IActionResult> ApplyCoupon(string code, string? email)
        {
            var result = await _cartService.ApplyCouponAsync(code, email);
            var cart = _cartService.GetCartViewModel();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = result.success,
                    message = result.message,
                    discountPercent = result.discountPercent,
                    discountAmount = cart.DiscountAmount.ToString("F2"),
                    total = cart.Total.ToString("F2")
                });
            }

            TempData[result.success ? "SuccessMessage" : "ErrorMessage"] = result.message;
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult RemoveCoupon()
        {
            _cartService.RemoveCoupon();
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public IActionResult GetCartSummary()
        {
            var cart = _cartService.GetCartViewModel();
            return Json(new
            {
                totalItems = cart.TotalItems,
                subtotal = cart.Subtotal.ToString("F2"),
                discount = cart.DiscountAmount.ToString("F2"),
                total = cart.Total.ToString("F2"),
                items = cart.Items
            });
        }
    }
}
