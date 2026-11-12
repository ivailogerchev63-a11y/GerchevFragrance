using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC.Intro.Data;
using MVC.Intro.Models;

namespace MVC.Intro.Controllers
{
    public class ProductController : Controller
    {
        private readonly AppDbContext _context;

        public ProductController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string? category, string? search, string? filter)
        {
            var query = _context.Products.Include(p => p.Variants).AsQueryable();

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(p => p.Category == category);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim().ToLower();
                query = query.Where(p =>
                    p.Name.ToLower().Contains(term) ||
                    (p.Brand != null && p.Brand.ToLower().Contains(term)) ||
                    (p.Concentration != null && p.Concentration.ToLower().Contains(term)));
            }

            if (filter == "bestsellers")
                query = query.Where(p => p.IsBestseller);
            else if (filter == "offers")
                query = query.Where(p => p.OriginalPrice != null && p.OriginalPrice > p.Price);

            var products = await query.OrderByDescending(p => p.IsBestseller).ThenByDescending(p => p.IsFeatured).ToListAsync();
            ViewBag.CurrentCategory = category;
            ViewBag.SearchTerm = search;
            ViewBag.CurrentFilter = filter;
            return View(products);
        }

        public async Task<IActionResult> Details(Guid id)
        {
            var product = await _context.Products
                .Include(p => p.Variants.OrderBy(v => v.SizeMl))
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null) return NotFound();

            var related = await _context.Products
                .Include(p => p.Variants)
                .Where(p => p.Id != id && (p.Brand == product.Brand || p.Category == product.Category))
                .Take(3)
                .ToListAsync();

            ViewBag.RelatedProducts = related;
            return View(product);
        }
    }
}
