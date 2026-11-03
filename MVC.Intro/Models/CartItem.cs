namespace MVC.Intro.Models
{
    /// <summary>
    /// A single item in the shopping cart (stored in Session as JSON).
    /// </summary>
    public class CartItem
    {
        public Guid ProductId { get; set; }
        public Guid ProductSizeId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public string Concentration { get; set; } = string.Empty;
        public int Ml { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; } = 1;

        public decimal LineTotal => UnitPrice * Quantity;
    }

    public class CartViewModel
    {
        public List<CartItem> Items { get; set; } = new();
        public decimal Total => Items.Sum(i => i.LineTotal);
        public int TotalItems => Items.Sum(i => i.Quantity);
    }
}
