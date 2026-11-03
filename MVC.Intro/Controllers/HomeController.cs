using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC.Intro.Data;
using MVC.Intro.Models;

namespace MVC.Intro.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var allProducts = await _context.Products.ToListAsync();

            // Featured fragrance: Dior Sauvage Elixir or fallback to first featured
            var featured = allProducts.FirstOrDefault(p => p.IsFeatured)
                           ?? allProducts.FirstOrDefault(p => p.ImageUrl == "/images/images.jpg")
                           ?? allProducts.FirstOrDefault();

            // Bestsellers
            var bestsellers = allProducts.Where(p => p.IsBestseller).ToList();
            if (!bestsellers.Any())
            {
                bestsellers = allProducts.OrderByDescending(p => p.Price).Take(4).ToList();
            }

            // Special Offers: genuine products with active discounts in database
            var offers = allProducts.Where(p => p.HasDiscount).OrderByDescending(p => p.DiscountPercentage).ToList();

            // Category card models
            var categories = new List<CategoryCardModel>
            {
                new CategoryCardModel
                {
                    Title = "МЪЖКИ ПАРФЮМИ",
                    Subtitle = "Смели, харизматични и класически аромати",
                    ImageUrl = "/images/images.jpg",
                    Url = Url.Action("Index", "Product", new { category = "Men" }) ?? "/Product?category=Men",
                    ProductCount = allProducts.Count(p => p.Category == "Men")
                },
                new CategoryCardModel
                {
                    Title = "ДАМСКИ АРОМАТИ",
                    Subtitle = "Изискана чувственост и флорална нежност",
                    ImageUrl = "/images/Gemini_Generated_Image_izenq4izenq4izen.jpg",
                    Url = Url.Action("Index", "Product", new { category = "Women" }) ?? "/Product?category=Women",
                    ProductCount = allProducts.Count(p => p.Category == "Women")
                },
                new CategoryCardModel
                {
                    Title = "УНИСЕКС КОЛЕКЦИЯ",
                    Subtitle = "Безгранична елегантност за ценители",
                    ImageUrl = "/images/images (1).jpg",
                    Url = Url.Action("Index", "Product", new { category = "Unisex" }) ?? "/Product?category=Unisex",
                    ProductCount = allProducts.Count(p => p.Category == "Unisex")
                },
                new CategoryCardModel
                {
                    Title = "ЛУКСОЗНИ & НИШОВИ",
                    Subtitle = "Рядка селекция от шедьоври на парфюмерията",
                    ImageUrl = "/images/Gemini_Generated_Image_8993a18993a18993.jpg",
                    Url = Url.Action("Index", "Product", new { category = "Luxury Fragrances" }) ?? "/Product?category=Luxury%20Fragrances",
                    ProductCount = allProducts.Count(p => p.Category == "Luxury Fragrances" || p.Price > 400)
                },
                new CategoryCardModel
                {
                    Title = "НОВИ ПОПЪЛНЕНИЯ",
                    Subtitle = "Най-новите попълнения в нашия бутик",
                    ImageUrl = "/images/Gemini_Generated_Image_pmsttypmsttypmst.jpg",
                    Url = Url.Action("Index", "Product", new { filter = "new" }) ?? "/Product?filter=new",
                    ProductCount = allProducts.Count
                }
            };

            var viewModel = new HomeViewModel
            {
                FeaturedFragrance = featured,
                Bestsellers = bestsellers,
                SpecialOffers = offers,
                AllProducts = allProducts,
                Categories = categories,
                ActivePromoCode = "GERCHEV10"
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
