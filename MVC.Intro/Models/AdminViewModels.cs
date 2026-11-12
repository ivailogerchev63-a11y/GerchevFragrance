using System.ComponentModel.DataAnnotations;

namespace MVC.Intro.Models
{
    // ─── Dashboard ─────────────────────────────────────────────────────────────

    public class AdminDashboardViewModel
    {
        public int TotalProducts { get; set; }
        public int AvailableProducts { get; set; }
        public int LowStockProducts { get; set; }
        public int TotalOrders { get; set; }
        public int TotalCustomers { get; set; }
        public decimal TotalRevenue { get; set; }
        public List<Order> RecentOrders { get; set; } = new();
        public List<AdminAuditLog> RecentActivity { get; set; } = new();
    }

    // ─── Products ──────────────────────────────────────────────────────────────

    public class AdminProductListViewModel
    {
        public List<Product> Products { get; set; } = new();
        public string? Search { get; set; }
        public string? Category { get; set; }
        public int Page { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }

    public class AdminProductEditViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Името е задължително")]
        [StringLength(200)]
        [Display(Name = "Наименование")]
        public string Name { get; set; } = string.Empty;

        [StringLength(2000)]
        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Required]
        [Range(0.01, 99999.99, ErrorMessage = "Невалидна цена")]
        [Display(Name = "Основна цена (EUR)")]
        public decimal Price { get; set; }

        [Range(0, 99999.99)]
        [Display(Name = "Оригинална цена (EUR)")]
        public decimal? OriginalPrice { get; set; }

        [StringLength(500)]
        [Display(Name = "URL на изображение")]
        public string? ImageUrl { get; set; }

        [StringLength(100)]
        [Display(Name = "Марка")]
        public string? Brand { get; set; }

        [StringLength(50)]
        [Display(Name = "Размер по подразбиране")]
        public string? Size { get; set; }

        [StringLength(50)]
        [Display(Name = "Концентрация")]
        public string? Concentration { get; set; }

        [Range(0, 99999)]
        [Display(Name = "Наличност")]
        public int StockQuantity { get; set; }

        [StringLength(50)]
        [Display(Name = "Категория")]
        public string? Category { get; set; }

        [Display(Name = "Бестселър")]
        public bool IsBestseller { get; set; }

        [Display(Name = "Препоръчан")]
        public bool IsFeatured { get; set; }

        [StringLength(1000)]
        [Display(Name = "Нотки")]
        public string? Notes { get; set; }

        [StringLength(100)]
        [Display(Name = "Ароматно семейство")]
        public string? FragranceFamily { get; set; }

        // For image upload — not stored in DB, used only during POST
        public IFormFile? ImageFile { get; set; }
    }

    // ─── Product Variants ──────────────────────────────────────────────────────

    public class AdminVariantEditViewModel
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }

        // Parent product name for display
        public string ProductName { get; set; } = string.Empty;

        [Required]
        [Range(1, 9999, ErrorMessage = "Въведете валиден обем в мл")]
        [Display(Name = "Обем (ml)")]
        public int SizeMl { get; set; }

        [Required]
        [Range(0.01, 99999.99, ErrorMessage = "Въведете валидна цена")]
        [Display(Name = "Цена (EUR)")]
        public decimal Price { get; set; }

        [Range(0, 99999.99)]
        [Display(Name = "Оригинална цена (EUR)")]
        public decimal? OriginalPrice { get; set; }

        [Range(0, 99999)]
        [Display(Name = "Наличност")]
        public int StockQuantity { get; set; }

        [Display(Name = "Наличен")]
        public bool IsAvailable { get; set; } = true;

        public List<ProductVariant> ExistingVariants { get; set; } = new();
    }

    // ─── Orders ────────────────────────────────────────────────────────────────

    public class AdminOrderListViewModel
    {
        public List<Order> Orders { get; set; } = new();
        public string? Search { get; set; }
        public string? Status { get; set; }
        public string? DateFrom { get; set; }
        public string? DateTo { get; set; }
        public int Page { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }

    public class AdminOrderStatusViewModel
    {
        public Guid OrderId { get; set; }

        [Required]
        public string NewStatus { get; set; } = string.Empty;
    }

    // ─── Customers ─────────────────────────────────────────────────────────────

    public class AdminCustomerListViewModel
    {
        public List<AdminCustomerRow> Customers { get; set; } = new();
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }

    public class AdminCustomerRow
    {
        public string Id { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string FullName => $"{FirstName} {LastName}".Trim();
        public DateTime? RegisteredAt { get; set; }
        public int OrderCount { get; set; }
        public decimal TotalSpent { get; set; }
    }

    // ─── Discounts ─────────────────────────────────────────────────────────────

    public class AdminDiscountListViewModel
    {
        public List<DiscountCoupon> Coupons { get; set; } = new();
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int TotalPages { get; set; }
        public int TotalCount { get; set; }
    }

    public class AdminDiscountEditViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Кодът е задължителен")]
        [StringLength(50)]
        [RegularExpression(@"^[A-Z0-9_\-]+$", ErrorMessage = "Само главни букви, цифри, '_' и '-'")]
        [Display(Name = "Код")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Имейлът е задължителен")]
        [EmailAddress(ErrorMessage = "Невалиден имейл")]
        [Display(Name = "Имейл на получателя")]
        public string Email { get; set; } = string.Empty;

        [Required]
        [Range(1, 100, ErrorMessage = "Отстъпката трябва да е между 1% и 100%")]
        [Display(Name = "Отстъпка (%)")]
        public decimal DiscountPercentage { get; set; } = 10;

        [Required]
        [Display(Name = "Валиден от")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [Display(Name = "Изтича на")]
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(30);

        [Display(Name = "Използван")]
        public bool IsUsed { get; set; }
    }

    // ─── Newsletter subscribers ─────────────────────────────────────────────────

    public class AdminNewsletterListViewModel
    {
        public List<DiscountCoupon> Subscribers { get; set; } = new();
        public int TotalSubscribers { get; set; }
        public int ActiveCoupons { get; set; }
        public int UsedCoupons { get; set; }
        public int ExpiredCoupons { get; set; }
    }
}
