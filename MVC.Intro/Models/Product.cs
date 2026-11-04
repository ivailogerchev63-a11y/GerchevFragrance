namespace MVC.Intro.Models
{
    public class Product
    {
        public Guid Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public decimal? OriginalPrice { get; set; }
        public string? ImageUrl { get; set; }
        public string? Brand { get; set; }
        public string? Size { get; set; }
        public string? Concentration { get; set; }
        public int StockQuantity { get; set; }
        public string? Category { get; set; } = "Men";
        public bool IsBestseller { get; set; } = false;
        public bool IsFeatured { get; set; } = false;
        public string? Notes { get; set; }

        public bool HasDiscount => OriginalPrice.HasValue && OriginalPrice.Value > Price;
        public int DiscountPercentage => HasDiscount ? (int)Math.Round(((OriginalPrice!.Value - Price) / OriginalPrice.Value) * 100m) : 0;
        public decimal SavingsAmount => HasDiscount ? (OriginalPrice!.Value - Price) : 0m;
    }
}
