namespace MVC.Intro.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }

        /// <summary>
        /// Default/fallback price in EUR. Used when no variant is selected.
        /// For products with variants, the variant price takes precedence.
        /// </summary>
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public string? ImageUrl { get; set; }
        public string? Brand { get; set; }

        /// <summary>
        /// Default size string for display (e.g. "75 ml").
        /// For products with variants, see ProductVariants.
        /// </summary>
        public string? Size { get; set; }
        public string? Concentration { get; set; }
        public int StockQuantity { get; set; }
        public string? Category { get; set; } = "Men";
        public bool IsBestseller { get; set; } = false;
        public bool IsFeatured { get; set; } = false;
        public string? Notes { get; set; }

        /// <summary>
        /// Fragrance family (e.g. "Ориенталски", "Фужер", "Флорален")
        /// </summary>
        public string? FragranceFamily { get; set; }

        /// <summary>Available bottle size variants with individual prices</summary>
        public List<ProductVariant> Variants { get; set; } = new();

        public bool HasDiscount => OriginalPrice.HasValue && OriginalPrice.Value > Price;
        public int DiscountPercentage => HasDiscount ? (int)Math.Round(((OriginalPrice!.Value - Price) / OriginalPrice.Value) * 100m) : 0;
        public decimal SavingsAmount => HasDiscount ? (OriginalPrice!.Value - Price) : 0m;

        /// <summary>Returns the lowest available variant price, or the product's base price.</summary>
        public decimal StartingPrice => Variants.Any()
            ? Variants.Where(v => v.IsAvailable).Min(v => v.Price)
            : Price;
    }
}
