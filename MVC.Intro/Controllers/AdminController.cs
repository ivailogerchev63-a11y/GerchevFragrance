using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC.Intro.Data;
using MVC.Intro.Models;

namespace MVC.Intro.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _db;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<AdminController> _logger;

        private static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
        private const int MaxImageSizeBytes = 5 * 1024 * 1024; // 5 MB
        private const int PageSize = 20;

        // Valid order statuses
        public static readonly string[] OrderStatuses =
        {
            "Потвърдена", "Обработва се", "Изпратена", "Доставена", "Отказана"
        };

        public AdminController(
            AppDbContext db,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env,
            ILogger<AdminController> logger)
        {
            _db = db;
            _userManager = userManager;
            _env = env;
            _logger = logger;
        }

        // ─── DASHBOARD ─────────────────────────────────────────────────────────

        public async Task<IActionResult> Index()
        {
            var totalProducts = await _db.Products.CountAsync();
            var availableProducts = await _db.Products.Where(p => p.StockQuantity > 0).CountAsync();
            var lowStock = await _db.Products.Where(p => p.StockQuantity > 0 && p.StockQuantity <= 3).CountAsync();
            var totalOrders = await _db.Orders.CountAsync();
            var totalCustomers = await _userManager.Users.CountAsync();
            var totalRevenue = await _db.Orders
                .Where(o => o.Status != "Отказана")
                .SumAsync(o => (decimal?)o.TotalAmount) ?? 0m;

            var recentOrders = await _db.Orders
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToListAsync();

            var recentActivity = await _db.AdminAuditLogs
                .OrderByDescending(a => a.CreatedAt)
                .Take(10)
                .ToListAsync();

            var vm = new AdminDashboardViewModel
            {
                TotalProducts = totalProducts,
                AvailableProducts = availableProducts,
                LowStockProducts = lowStock,
                TotalOrders = totalOrders,
                TotalCustomers = totalCustomers,
                TotalRevenue = totalRevenue,
                RecentOrders = recentOrders,
                RecentActivity = recentActivity
            };

            return View(vm);
        }

        // ─── PRODUCTS LIST ──────────────────────────────────────────────────────

        public async Task<IActionResult> Products(string? search, string? category, int page = 1)
        {
            var query = _db.Products.Include(p => p.Variants).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(p => p.Name.Contains(search) || (p.Brand != null && p.Brand.Contains(search)));

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(p => p.Category == category);

            var total = await query.CountAsync();
            var products = await query
                .OrderBy(p => p.Brand).ThenBy(p => p.Name)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var vm = new AdminProductListViewModel
            {
                Products = products,
                Search = search,
                Category = category,
                Page = page,
                TotalPages = (int)Math.Ceiling(total / (double)PageSize),
                TotalCount = total
            };

            return View(vm);
        }

        // ─── CREATE PRODUCT ─────────────────────────────────────────────────────

        [HttpGet]
        public IActionResult CreateProduct()
        {
            return View(new AdminProductEditViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(AdminProductEditViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            string? imageUrl = null;

            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                var uploadResult = await SaveImageAsync(vm.ImageFile);
                if (uploadResult.error != null)
                {
                    ModelState.AddModelError("ImageFile", uploadResult.error);
                    return View(vm);
                }
                imageUrl = uploadResult.path;
            }
            else if (!string.IsNullOrWhiteSpace(vm.ImageUrl))
            {
                imageUrl = vm.ImageUrl;
            }

            var product = new Product
            {
                Id = Guid.NewGuid(),
                Name = vm.Name.Trim(),
                Description = vm.Description?.Trim(),
                Price = vm.Price,
                OriginalPrice = vm.OriginalPrice > 0 ? vm.OriginalPrice : null,
                ImageUrl = imageUrl,
                Brand = vm.Brand?.Trim(),
                Size = vm.Size?.Trim(),
                Concentration = vm.Concentration?.Trim(),
                StockQuantity = vm.StockQuantity,
                Category = vm.Category?.Trim() ?? "Men",
                IsBestseller = vm.IsBestseller,
                IsFeatured = vm.IsFeatured,
                Notes = vm.Notes?.Trim(),
                FragranceFamily = vm.FragranceFamily?.Trim()
            };

            _db.Products.Add(product);
            await _db.SaveChangesAsync();
            await LogAuditAsync("ProductCreated", $"Добавен продукт: {product.Name}", "Product", product.Id.ToString());

            TempData["SuccessMessage"] = $"Продуктът \"{product.Name}\" беше добавен успешно.";
            return RedirectToAction(nameof(Products));
        }

        // ─── EDIT PRODUCT ───────────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> EditProduct(Guid id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            var vm = new AdminProductEditViewModel
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                OriginalPrice = product.OriginalPrice,
                ImageUrl = product.ImageUrl,
                Brand = product.Brand,
                Size = product.Size,
                Concentration = product.Concentration,
                StockQuantity = product.StockQuantity,
                Category = product.Category,
                IsBestseller = product.IsBestseller,
                IsFeatured = product.IsFeatured,
                Notes = product.Notes,
                FragranceFamily = product.FragranceFamily
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(AdminProductEditViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var product = await _db.Products.FindAsync(vm.Id);
            if (product == null) return NotFound();

            // Handle image upload
            if (vm.ImageFile != null && vm.ImageFile.Length > 0)
            {
                var uploadResult = await SaveImageAsync(vm.ImageFile);
                if (uploadResult.error != null)
                {
                    ModelState.AddModelError("ImageFile", uploadResult.error);
                    return View(vm);
                }
                product.ImageUrl = uploadResult.path;
            }
            else if (!string.IsNullOrWhiteSpace(vm.ImageUrl))
            {
                product.ImageUrl = vm.ImageUrl;
            }

            product.Name = vm.Name.Trim();
            product.Description = vm.Description?.Trim();
            product.Price = vm.Price;
            product.OriginalPrice = vm.OriginalPrice > 0 ? vm.OriginalPrice : null;
            product.Brand = vm.Brand?.Trim();
            product.Size = vm.Size?.Trim();
            product.Concentration = vm.Concentration?.Trim();
            product.StockQuantity = vm.StockQuantity;
            product.Category = vm.Category?.Trim() ?? "Men";
            product.IsBestseller = vm.IsBestseller;
            product.IsFeatured = vm.IsFeatured;
            product.Notes = vm.Notes?.Trim();
            product.FragranceFamily = vm.FragranceFamily?.Trim();

            await _db.SaveChangesAsync();
            await LogAuditAsync("ProductEdited", $"Редактиран продукт: {product.Name}", "Product", product.Id.ToString());

            TempData["SuccessMessage"] = $"Продуктът \"{product.Name}\" беше обновен успешно.";
            return RedirectToAction(nameof(Products));
        }

        // ─── DELETE PRODUCT ─────────────────────────────────────────────────────

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            // Check if product has order history
            var hasOrders = await _db.OrderItems.AnyAsync(oi => oi.ProductId == id);
            if (hasOrders)
            {
                // Soft-delete: set stock to 0 and unfeature instead of deleting
                product.StockQuantity = 0;
                product.IsFeatured = false;
                product.IsBestseller = false;
                await _db.SaveChangesAsync();
                await LogAuditAsync("ProductDeactivated", $"Деактивиран продукт (има поръчки): {product.Name}", "Product", id.ToString());
                TempData["SuccessMessage"] = $"Продуктът \"{product.Name}\" беше деактивиран (има история в поръчки).";
            }
            else
            {
                var name = product.Name;
                _db.Products.Remove(product);
                await _db.SaveChangesAsync();
                await LogAuditAsync("ProductDeleted", $"Изтрит продукт: {name}", "Product", id.ToString());
                TempData["SuccessMessage"] = $"Продуктът \"{name}\" беше изтрит.";
            }

            return RedirectToAction(nameof(Products));
        }

        // ─── PRODUCT VARIANTS ───────────────────────────────────────────────────

        [HttpGet]
        public async Task<IActionResult> Variants(Guid productId)
        {
            var product = await _db.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == productId);

            if (product == null) return NotFound();

            var vm = new AdminVariantEditViewModel
            {
                ProductId = productId,
                ProductName = product.Name,
                ExistingVariants = product.Variants.OrderBy(v => v.SizeMl).ToList(),
                IsAvailable = true
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddVariant(AdminVariantEditViewModel vm)
        {
            var product = await _db.Products
                .Include(p => p.Variants)
                .FirstOrDefaultAsync(p => p.Id == vm.ProductId);

            if (product == null) return NotFound();

            // Re-populate display fields
            vm.ProductName = product.Name;
            vm.ExistingVariants = product.Variants.OrderBy(v => v.SizeMl).ToList();

            if (!ModelState.IsValid)
                return View("Variants", vm);

            // Check for duplicate size
            if (product.Variants.Any(v => v.SizeMl == vm.SizeMl))
            {
                ModelState.AddModelError("SizeMl", $"Вариант {vm.SizeMl} ml вече съществува за този продукт.");
                return View("Variants", vm);
            }

            var variant = new ProductVariant
            {
                Id = Guid.NewGuid(),
                ProductId = vm.ProductId,
                SizeMl = vm.SizeMl,
                Price = vm.Price,
                OriginalPrice = vm.OriginalPrice > 0 ? vm.OriginalPrice : null,
                StockQuantity = vm.StockQuantity,
                IsAvailable = vm.IsAvailable
            };

            _db.ProductVariants.Add(variant);
            await _db.SaveChangesAsync();
            await LogAuditAsync("VariantAdded", $"Добавен вариант {variant.SizeMl} ml за {product.Name}", "ProductVariant", variant.Id.ToString());

            TempData["SuccessMessage"] = $"Вариантът {variant.SizeMl} ml беше добавен успешно.";
            return RedirectToAction(nameof(Variants), new { productId = vm.ProductId });
        }

        [HttpGet]
        public async Task<IActionResult> EditVariant(Guid id)
        {
            var variant = await _db.ProductVariants
                .Include(v => v.Product)
                    .ThenInclude(p => p!.Variants)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (variant == null) return NotFound();

            var vm = new AdminVariantEditViewModel
            {
                Id = variant.Id,
                ProductId = variant.ProductId,
                ProductName = variant.Product?.Name ?? "",
                SizeMl = variant.SizeMl,
                Price = variant.Price,
                OriginalPrice = variant.OriginalPrice,
                StockQuantity = variant.StockQuantity,
                IsAvailable = variant.IsAvailable,
                ExistingVariants = variant.Product?.Variants.OrderBy(v => v.SizeMl).ToList() ?? new()
            };

            return View("EditVariant", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditVariant(AdminVariantEditViewModel vm)
        {
            var variant = await _db.ProductVariants
                .Include(v => v.Product)
                    .ThenInclude(p => p!.Variants)
                .FirstOrDefaultAsync(v => v.Id == vm.Id);

            if (variant == null) return NotFound();

            vm.ProductName = variant.Product?.Name ?? "";
            vm.ExistingVariants = variant.Product?.Variants.OrderBy(v => v.SizeMl).ToList() ?? new();

            if (!ModelState.IsValid)
                return View("EditVariant", vm);

            variant.SizeMl = vm.SizeMl;
            variant.Price = vm.Price;
            variant.OriginalPrice = vm.OriginalPrice > 0 ? vm.OriginalPrice : null;
            variant.StockQuantity = vm.StockQuantity;
            variant.IsAvailable = vm.IsAvailable;

            await _db.SaveChangesAsync();
            await LogAuditAsync("VariantEdited", $"Редактиран вариант {variant.SizeMl} ml за {variant.Product?.Name}", "ProductVariant", variant.Id.ToString());

            TempData["SuccessMessage"] = "Вариантът беше обновен успешно.";
            return RedirectToAction(nameof(Variants), new { productId = vm.ProductId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVariant(Guid id, Guid productId)
        {
            var variant = await _db.ProductVariants.FindAsync(id);
            if (variant == null) return NotFound();

            var sizeMl = variant.SizeMl;
            _db.ProductVariants.Remove(variant);
            await _db.SaveChangesAsync();
            await LogAuditAsync("VariantDeleted", $"Изтрит вариант {sizeMl} ml", "ProductVariant", id.ToString());

            TempData["SuccessMessage"] = $"Вариантът {sizeMl} ml беше изтрит.";
            return RedirectToAction(nameof(Variants), new { productId });
        }

        // ─── ORDERS ─────────────────────────────────────────────────────────────

        public async Task<IActionResult> Orders(string? search, string? status, string? dateFrom, string? dateTo, int page = 1)
        {
            var query = _db.Orders.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(o => o.OrderNumber.Contains(search) ||
                                         o.CustomerName.Contains(search) ||
                                         o.CustomerEmail.Contains(search));

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(o => o.Status == status);

            if (!string.IsNullOrWhiteSpace(dateFrom) && DateTime.TryParse(dateFrom, out var from))
                query = query.Where(o => o.OrderDate >= from);

            if (!string.IsNullOrWhiteSpace(dateTo) && DateTime.TryParse(dateTo, out var to))
                query = query.Where(o => o.OrderDate <= to.AddDays(1));

            var total = await query.CountAsync();
            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var vm = new AdminOrderListViewModel
            {
                Orders = orders,
                Search = search,
                Status = status,
                DateFrom = dateFrom,
                DateTo = dateTo,
                Page = page,
                TotalPages = (int)Math.Ceiling(total / (double)PageSize),
                TotalCount = total
            };

            ViewBag.OrderStatuses = OrderStatuses;
            return View(vm);
        }

        public async Task<IActionResult> OrderDetails(Guid id)
        {
            var order = await _db.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();

            ViewBag.OrderStatuses = OrderStatuses;
            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(AdminOrderStatusViewModel vm)
        {
            if (!OrderStatuses.Contains(vm.NewStatus))
            {
                TempData["ErrorMessage"] = "Невалиден статус.";
                return RedirectToAction(nameof(OrderDetails), new { id = vm.OrderId });
            }

            var order = await _db.Orders.FindAsync(vm.OrderId);
            if (order == null) return NotFound();

            var oldStatus = order.Status;
            order.Status = vm.NewStatus;
            await _db.SaveChangesAsync();
            await LogAuditAsync("OrderStatusChanged",
                $"Поръчка {order.OrderNumber}: {oldStatus} → {vm.NewStatus}", "Order", order.Id.ToString());

            TempData["SuccessMessage"] = $"Статусът на поръчка {order.OrderNumber} беше променен на \"{vm.NewStatus}\".";
            return RedirectToAction(nameof(OrderDetails), new { id = vm.OrderId });
        }

        // ─── CUSTOMERS ──────────────────────────────────────────────────────────

        public async Task<IActionResult> Customers(string? search, int page = 1)
        {
            var usersQuery = _userManager.Users.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                usersQuery = usersQuery.Where(u =>
                    u.Email!.Contains(search) ||
                    (u.FirstName != null && u.FirstName.Contains(search)) ||
                    (u.LastName != null && u.LastName.Contains(search)));

            var total = await usersQuery.CountAsync();
            var users = await usersQuery
                .OrderByDescending(u => u.Id)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            // Build rows with order stats
            var userEmails = users.Select(u => u.Email!.ToLower()).ToList();
            var orderStats = await _db.Orders
                .Where(o => userEmails.Contains(o.CustomerEmail.ToLower()))
                .GroupBy(o => o.CustomerEmail.ToLower())
                .Select(g => new { Email = g.Key, Count = g.Count(), Spent = g.Sum(o => o.TotalAmount) })
                .ToListAsync();

            var rows = users.Select(u => new AdminCustomerRow
            {
                Id = u.Id,
                Email = u.Email ?? "",
                FirstName = u.FirstName,
                LastName = u.LastName,
                OrderCount = orderStats.FirstOrDefault(s => s.Email == (u.Email ?? "").ToLower())?.Count ?? 0,
                TotalSpent = orderStats.FirstOrDefault(s => s.Email == (u.Email ?? "").ToLower())?.Spent ?? 0m
            }).ToList();

            var vm = new AdminCustomerListViewModel
            {
                Customers = rows,
                Search = search,
                Page = page,
                TotalPages = (int)Math.Ceiling(total / (double)PageSize),
                TotalCount = total
            };

            return View(vm);
        }

        public async Task<IActionResult> CustomerDetails(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var orders = await _db.Orders
                .Include(o => o.Items)
                .Where(o => o.CustomerEmail.ToLower() == (user.Email ?? "").ToLower())
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            ViewBag.User = user;
            ViewBag.Orders = orders;
            return View();
        }

        // ─── DISCOUNTS ──────────────────────────────────────────────────────────

        public async Task<IActionResult> Discounts(string? search, int page = 1)
        {
            var query = _db.DiscountCoupons.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
                query = query.Where(c => c.Code.Contains(search) || c.Email.Contains(search));

            var total = await query.CountAsync();
            var coupons = await query
                .OrderByDescending(c => c.CreatedAt)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            var vm = new AdminDiscountListViewModel
            {
                Coupons = coupons,
                Search = search,
                Page = page,
                TotalPages = (int)Math.Ceiling(total / (double)PageSize),
                TotalCount = total
            };

            return View(vm);
        }

        [HttpGet]
        public IActionResult CreateDiscount()
        {
            return View(new AdminDiscountEditViewModel
            {
                Code = "GERCHEV",
                DiscountPercentage = 10,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(30)
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateDiscount(AdminDiscountEditViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var coupon = new DiscountCoupon
            {
                Id = Guid.NewGuid(),
                Code = vm.Code.Trim().ToUpperInvariant(),
                Email = vm.Email.Trim().ToLowerInvariant(),
                DiscountPercentage = vm.DiscountPercentage,
                CreatedAt = vm.CreatedAt,
                ExpiresAt = vm.ExpiresAt,
                IsUsed = false
            };

            _db.DiscountCoupons.Add(coupon);
            await _db.SaveChangesAsync();
            await LogAuditAsync("DiscountCreated", $"Създаден код: {coupon.Code} за {coupon.Email}", "DiscountCoupon", coupon.Id.ToString());

            TempData["SuccessMessage"] = $"Кодът \"{coupon.Code}\" беше създаден успешно.";
            return RedirectToAction(nameof(Discounts));
        }

        [HttpGet]
        public async Task<IActionResult> EditDiscount(Guid id)
        {
            var coupon = await _db.DiscountCoupons.FindAsync(id);
            if (coupon == null) return NotFound();

            var vm = new AdminDiscountEditViewModel
            {
                Id = coupon.Id,
                Code = coupon.Code,
                Email = coupon.Email,
                DiscountPercentage = coupon.DiscountPercentage,
                CreatedAt = coupon.CreatedAt,
                ExpiresAt = coupon.ExpiresAt,
                IsUsed = coupon.IsUsed
            };

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditDiscount(AdminDiscountEditViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var coupon = await _db.DiscountCoupons.FindAsync(vm.Id);
            if (coupon == null) return NotFound();

            coupon.Code = vm.Code.Trim().ToUpperInvariant();
            coupon.Email = vm.Email.Trim().ToLowerInvariant();
            coupon.DiscountPercentage = vm.DiscountPercentage;
            coupon.CreatedAt = vm.CreatedAt;
            coupon.ExpiresAt = vm.ExpiresAt;
            coupon.IsUsed = vm.IsUsed;

            await _db.SaveChangesAsync();
            await LogAuditAsync("DiscountEdited", $"Редактиран код: {coupon.Code}", "DiscountCoupon", coupon.Id.ToString());

            TempData["SuccessMessage"] = "Кодът беше обновен успешно.";
            return RedirectToAction(nameof(Discounts));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteDiscount(Guid id)
        {
            var coupon = await _db.DiscountCoupons.FindAsync(id);
            if (coupon == null) return NotFound();

            var code = coupon.Code;
            _db.DiscountCoupons.Remove(coupon);
            await _db.SaveChangesAsync();
            await LogAuditAsync("DiscountDeleted", $"Изтрит код: {code}", "DiscountCoupon", id.ToString());

            TempData["SuccessMessage"] = $"Кодът \"{code}\" беше изтрит.";
            return RedirectToAction(nameof(Discounts));
        }

        // ─── NEWSLETTER / SUBSCRIBERS ───────────────────────────────────────────

        public async Task<IActionResult> Newsletter()
        {
            var all = await _db.DiscountCoupons
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var now = DateTime.UtcNow;

            var vm = new AdminNewsletterListViewModel
            {
                Subscribers = all,
                TotalSubscribers = all.Select(c => c.Email).Distinct().Count(),
                ActiveCoupons = all.Count(c => !c.IsUsed && c.ExpiresAt > now),
                UsedCoupons = all.Count(c => c.IsUsed),
                ExpiredCoupons = all.Count(c => !c.IsUsed && c.ExpiresAt <= now)
            };

            return View(vm);
        }

        // ─── HOMEPAGE COLLECTIONS ───────────────────────────────────────────────

        public async Task<IActionResult> Collections()
        {
            var products = await _db.Products
                .OrderBy(p => p.Brand).ThenBy(p => p.Name)
                .ToListAsync();

            ViewBag.Products = products;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleBestseller(Guid id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.IsBestseller = !product.IsBestseller;
            await _db.SaveChangesAsync();
            await LogAuditAsync("BestsellerToggled",
                $"{product.Name}: IsBestseller = {product.IsBestseller}", "Product", id.ToString());

            return Json(new { success = true, isBestseller = product.IsBestseller });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleFeatured(Guid id)
        {
            var product = await _db.Products.FindAsync(id);
            if (product == null) return NotFound();

            product.IsFeatured = !product.IsFeatured;
            await _db.SaveChangesAsync();
            await LogAuditAsync("FeaturedToggled",
                $"{product.Name}: IsFeatured = {product.IsFeatured}", "Product", id.ToString());

            return Json(new { success = true, isFeatured = product.IsFeatured });
        }

        // ─── AUDIT LOG ──────────────────────────────────────────────────────────

        public async Task<IActionResult> AuditLog(int page = 1)
        {
            var total = await _db.AdminAuditLogs.CountAsync();
            var logs = await _db.AdminAuditLogs
                .OrderByDescending(l => l.CreatedAt)
                .Skip((page - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();

            ViewBag.Logs = logs;
            ViewBag.Page = page;
            ViewBag.TotalPages = (int)Math.Ceiling(total / (double)PageSize);
            ViewBag.TotalCount = total;

            return View();
        }

        // ─── IMAGE UPLOAD HELPER ────────────────────────────────────────────────

        private async Task<(string? path, string? error)> SaveImageAsync(IFormFile file)
        {
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!AllowedImageExtensions.Contains(ext))
                return (null, "Разрешени формати: JPG, JPEG, PNG, WebP.");

            if (file.Length > MaxImageSizeBytes)
                return (null, "Файлът е прекалено голям. Максималният размер е 5 MB.");

            // Verify it's actually an image by reading the first few bytes
            await using var stream = file.OpenReadStream();
            var header = new byte[12];
            var read = await stream.ReadAsync(header, 0, 12);
            stream.Position = 0;

            if (!IsImageBytes(header, read))
                return (null, "Файлът не е валидно изображение.");

            var safeFileName = $"{Guid.NewGuid()}{ext}";
            var imagesDir = Path.Combine(_env.WebRootPath, "images");
            Directory.CreateDirectory(imagesDir);
            var fullPath = Path.Combine(imagesDir, safeFileName);

            await using var dest = System.IO.File.Create(fullPath);
            stream.Position = 0;
            await file.CopyToAsync(dest);

            return ($"/images/{safeFileName}", null);
        }

        private static bool IsImageBytes(byte[] header, int length)
        {
            if (length < 4) return false;
            // JPEG: FF D8 FF
            if (header[0] == 0xFF && header[1] == 0xD8 && header[2] == 0xFF) return true;
            // PNG: 89 50 4E 47
            if (header[0] == 0x89 && header[1] == 0x50 && header[2] == 0x4E && header[3] == 0x47) return true;
            // WebP: 52 49 46 46 ... 57 45 42 50
            if (length >= 12 && header[0] == 0x52 && header[1] == 0x49 && header[2] == 0x46 && header[3] == 0x46
                && header[8] == 0x57 && header[9] == 0x45 && header[10] == 0x42 && header[11] == 0x50)
                return true;

            return false;
        }

        // ─── AUDIT LOG HELPER ───────────────────────────────────────────────────

        private async Task LogAuditAsync(string action, string description, string? entityType = null, string? entityId = null)
        {
            var email = User.Identity?.Name ?? "unknown";
            _db.AdminAuditLogs.Add(new AdminAuditLog
            {
                Action = action,
                Description = description,
                AdminEmail = email,
                EntityType = entityType,
                EntityId = entityId,
                CreatedAt = DateTime.UtcNow
            });
            await _db.SaveChangesAsync();
        }
    }
}
