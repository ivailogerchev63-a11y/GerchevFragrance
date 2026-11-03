using Microsoft.AspNetCore.Mvc;
using MVC.Intro.Services;

namespace MVC.Intro.Controllers
{
    public class CartController : Controller
    {
        private readonly CartService _cart;

        public CartController(CartService cart)
        {
            _cart = cart;
        }

        // GET /Cart
        public IActionResult Index()
        {
            return View(_cart.GetCart());
        }

        // POST /Cart/UpdateQuantity
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(Guid productSizeId, int quantity)
        {
            _cart.UpdateQuantity(productSizeId, quantity);
            return RedirectToAction(nameof(Index));
        }

        // POST /Cart/Remove
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Remove(Guid productSizeId)
        {
            _cart.RemoveItem(productSizeId);
            TempData["Success"] = "Артикулът е премахнат от количката.";
            return RedirectToAction(nameof(Index));
        }

        // POST /Cart/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            _cart.Clear();
            TempData["Success"] = "Количката е изчистена.";
            return RedirectToAction(nameof(Index));
        }

        // GET /Cart/Checkout
        public IActionResult Checkout()
        {
            var cart = _cart.GetCart();
            if (!cart.Items.Any())
                return RedirectToAction(nameof(Index));

            return View(cart);
        }
    }
}
