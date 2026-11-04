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

        /// <summary>Sum of all line totals before any discount.</summary>
        public decimal Subtotal => Items.Sum(i => i.LineTotal);

        /// <summary>Applied coupon code (null = no coupon).</summary>
        public string? CouponCode { get; set; }

        /// <summary>Discount percent (0–100).</summary>
        public int DiscountPercent { get; set; } = 0;

        /// <summary>Monetary discount amount.</summary>
        public decimal DiscountAmount => DiscountPercent > 0
            ? Math.Round(Subtotal * DiscountPercent / 100m, 2)
            : 0m;

        /// <summary>Final total after discount.</summary>
        public decimal Total => Subtotal - DiscountAmount;

        /// <summary>Backwards-compat alias used by old views.</summary>
        public int TotalItems => Items.Sum(i => i.Quantity);
    }

    // ── Checkout form model ───────────────────────────────────────────────────

    public class CheckoutFormModel
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName  { get; set; } = string.Empty;
        public string Email     { get; set; } = string.Empty;
        public string? Phone    { get; set; }
        public string Address   { get; set; } = string.Empty;
        public string City      { get; set; } = string.Empty;
        public string? PostalCode { get; set; }
        public string? CouponCode { get; set; }
    }
}
