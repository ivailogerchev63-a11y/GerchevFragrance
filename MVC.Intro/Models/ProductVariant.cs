namespace MVC.Intro.Models
{
    /// <summary>
    /// Represents a specific bottle size/variant of a perfume product.
    /// Each product can have multiple variants with different sizes and prices.
    /// </summary>
    public class ProductVariant
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ProductId { get; set; }
        public Product? Product { get; set; }

        /// <summary>Bottle size in ml (e.g. 50, 75, 100, 125, 150, 200)</summary>
        public int SizeMl { get; set; }

        /// <summary>Price in EUR for this specific size</summary>
        public decimal Price { get; set; }

        /// <summary>Optional original/crossed-out price in EUR for discounted variants</summary>
        public decimal? OriginalPrice { get; set; }

        /// <summary>Available stock quantity for this variant</summary>
        public int StockQuantity { get; set; } = 10;

        public bool IsAvailable { get; set; } = true;

        public bool HasDiscount => OriginalPrice.HasValue && OriginalPrice.Value > Price;
        public int DiscountPercentage => HasDiscount
            ? (int)Math.Round(((OriginalPrice!.Value - Price) / OriginalPrice.Value) * 100m)
            : 0;
    }
}
