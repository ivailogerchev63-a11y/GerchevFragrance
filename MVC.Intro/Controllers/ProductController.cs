using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC.Intro.Data;
using MVC.Intro.Models;
using MVC.Intro.Services;

namespace MVC.Intro.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;
        private readonly CartService _cart;

        public ProductController(AppDbContext context, CartService cart)
        {
            _context = context;
            _cart = cart;
        }

        // GET /Product
        public async Task<IActionResult> Index()
        {
            var products = await _context.Products
                .Include(p => p.Sizes)
                .ToListAsync();
            return View(products);
        }

        // GET /Product/Details/{id}
        public async Task<IActionResult> Details(Guid id)
        {
            var product = await _context.Products
                .Include(p => p.Sizes)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();
            return View(product);
        }

        // GET /Product/Create
        public IActionResult Create()
        {
            return View(new Product { Name = string.Empty });
        }

        // POST /Product/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Product product)
        {
            // Remove navigation-property validation noise
            ModelState.Remove("Sizes");

            if (!ModelState.IsValid)
                return View(product);

            product.Id = Guid.NewGuid();

            // Add the three standard size variants with the base price
            product.Sizes = new List<ProductSize>
            {
                new ProductSize { Id = Guid.NewGuid(), Ml = 50,  Price = Math.Round(product.Price * 0.70m, 2) },
                new ProductSize { Id = Guid.NewGuid(), Ml = 100, Price = product.Price },
                new ProductSize { Id = Guid.NewGuid(), Ml = 150, Price = Math.Round(product.Price * 1.30m, 2) }
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Продуктът \"" + product.Name + "\" е добавен успешно.";
            return RedirectToAction(nameof(Index));
        }

        // POST /Product/AddToCart
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(Guid productId, Guid productSizeId, int quantity = 1)
        {
            if (quantity < 1) quantity = 1;

            var product = await _context.Products
                .Include(p => p.Sizes)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null) return NotFound();

            var size = product.Sizes.FirstOrDefault(s => s.Id == productSizeId);
            if (size == null || !size.IsAvailable)
            {
                TempData["Error"] = "Избраният размер не е наличен.";
                return RedirectToAction(nameof(Details), new { id = productId });
            }

            var item = new CartItem
            {
                ProductId      = product.Id,
                ProductSizeId  = size.Id,
                ProductName    = product.Name,
                Brand          = product.Brand ?? string.Empty,
                ImageUrl       = product.ImageUrl,
                Concentration  = product.Concentration ?? string.Empty,
                Ml             = size.Ml,
                UnitPrice      = size.Price,
                Quantity       = quantity
            };

            _cart.AddOrUpdate(item);
            TempData["Success"] = product.Name + " " + size.Ml + " ml е добавен в количката.";
            return RedirectToAction("Index", "Cart");
        }
    }
}
