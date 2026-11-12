using Microsoft.AspNetCore.Mvc;
using MVC.Intro.Helpers;
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
        public async Task<IActionResult> Add(Guid id, int quantity = 1, Guid? variantId = null)
        {
            var success = await _cartService.AddToCartAsync(id, quantity, variantId);
            var cart = _cartService.GetCartViewModel();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest" || Request.Headers.Accept.ToString().Contains("application/json"))
            {
                return Json(new
                {
                    success,
                    totalItems = cart.TotalItems,
                    subtotal = CurrencyHelper.FormatEuro(cart.Subtotal),
                    total = CurrencyHelper.FormatEuro(cart.Total),
                    message = success ? "Продуктът беше добавен в количката!" : "Възникна грешка при добавянето."
                });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult UpdateQuantity(Guid id, int quantity, string? cartKey = null)
        {
            if (!string.IsNullOrEmpty(cartKey))
                _cartService.UpdateQuantity(cartKey, quantity);
            else
                _cartService.UpdateQuantity(id, quantity);

            var cart = _cartService.GetCartViewModel();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    totalItems = cart.TotalItems,
                    subtotal = CurrencyHelper.FormatEuro(cart.Subtotal),
                    discount = CurrencyHelper.FormatEuro(cart.DiscountAmount),
                    total = CurrencyHelper.FormatEuro(cart.Total)
                });
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        public IActionResult Remove(Guid id, string? cartKey = null)
        {
            if (!string.IsNullOrEmpty(cartKey))
                _cartService.RemoveFromCart(cartKey);
            else
                _cartService.RemoveFromCart(id);

            var cart = _cartService.GetCartViewModel();

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return Json(new
                {
                    success = true,
                    totalItems = cart.TotalItems,
                    subtotal = CurrencyHelper.FormatEuro(cart.Subtotal),
                    total = CurrencyHelper.FormatEuro(cart.Total)
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
                    discountAmount = CurrencyHelper.FormatEuro(cart.DiscountAmount),
                    total = CurrencyHelper.FormatEuro(cart.Total)
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
                subtotal = CurrencyHelper.FormatEuro(cart.Subtotal),
                discount = CurrencyHelper.FormatEuro(cart.DiscountAmount),
                total = CurrencyHelper.FormatEuro(cart.Total),
                items = cart.Items
            });
        }
    }
}
