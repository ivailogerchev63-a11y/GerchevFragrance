using System.ComponentModel.DataAnnotations;

namespace MVC.Intro.Models
{
    public class Product
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Името е задължително")]
        [StringLength(200)]
        [Display(Name = "Наименование")]
        public required string Name { get; set; }

        [Display(Name = "Описание")]
        public string? Description { get; set; }

        [Display(Name = "Основна цена (EUR)")]
        [Range(0.01, 99999, ErrorMessage = "Цената трябва да е положително число")]
        public decimal Price { get; set; }

        [Display(Name = "Снимка (URL)")]
        public string? ImageUrl { get; set; }

        [Display(Name = "Марка")]
        public string? Brand { get; set; }

        // Legacy single-size field kept for display fallback; prefer ProductSizes
        [Display(Name = "Размер")]
        public string? Size { get; set; }

        [Display(Name = "Концентрация")]
        public string? Concentration { get; set; }

        [Display(Name = "Количество в наличност")]
        [Range(0, int.MaxValue)]
        public int StockQuantity { get; set; }

        // ── Fragrance family ────────────────────────────────────────────────────
        /// <summary>e.g. "Ориенталски — Уд", "Свеж — Морски", "Флорален"</summary>
        [Display(Name = "Ароматно семейство")]
        public string? FragranceFamily { get; set; }

        // ── Fragrance pyramid notes (comma-separated lists) ─────────────────────
        /// <summary>First impression notes, 15–30 min. Comma-separated.</summary>
        [Display(Name = "Начални ноти")]
        public string? TopNotes { get; set; }

        /// <summary>Heart of the fragrance, 30 min–4 h. Comma-separated.</summary>
        [Display(Name = "Сърдечни ноти")]
        public string? HeartNotes { get; set; }

        /// <summary>Dry-down notes, 4+ h. Comma-separated.</summary>
        [Display(Name = "Базови ноти")]
        public string? BaseNotes { get; set; }

        // ── Navigation ──────────────────────────────────────────────────────────
        public ICollection<ProductSize> Sizes { get; set; } = new List<ProductSize>();
    }

    /// <summary>
    /// Represents one size option (e.g. 50 ml) for a product with its own price and availability.
    /// </summary>
    public class ProductSize
    {
        public Guid Id { get; set; }

        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        /// <summary>Volume in ml (e.g. 50, 100, 150).</summary>
        [Range(1, 9999)]
        public int Ml { get; set; }

        /// <summary>Price for this specific size in EUR.</summary>
        [Range(0.01, 99999)]
        public decimal Price { get; set; }

        /// <summary>False when the manufacturer does not produce this size.</summary>
        public bool IsAvailable { get; set; } = true;

        /// <summary>
        /// When IsAvailable is false, explains why (e.g. "Не се произвежда в 150 ml").
        /// </summary>
        public string? UnavailableReason { get; set; }
    }
}
