namespace MVC.Intro.Models
{
    public class CartItem
    {
        public Guid ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Brand { get; set; } = string.Empty;

        /// <summary>Price in EUR for the selected size</summary>
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }

        /// <summary>The selected bottle size, e.g. "75 ml". Included in cart identity key.</summary>
        public string? Size { get; set; }
        public string? Concentration { get; set; }
        public int Quantity { get; set; } = 1;
        public decimal Subtotal => Price * Quantity;

        /// <summary>
        /// Composite identity key: same product in different sizes = different cart lines.
        /// </summary>
        public string CartKey => $"{ProductId}|{Size ?? "default"}";
    }

    public class CartViewModel
    {
        public const decimal FreeShippingThreshold = 150m;
        public const decimal StandardShippingFee = 6.00m;

        public List<CartItem> Items { get; set; } = new();
        public decimal Subtotal => Items.Sum(i => i.Subtotal);
        public string? AppliedCouponCode { get; set; }
        public decimal DiscountPercentage { get; set; }
        public decimal DiscountAmount => Subtotal * (DiscountPercentage / 100m);
        public decimal ShippingFee => Subtotal >= FreeShippingThreshold ? 0m : StandardShippingFee;
        public decimal Total => GetOrderTotal(DiscountAmount);
        public int TotalItems => Items.Sum(i => i.Quantity);

        public decimal GetOrderTotal(decimal discountAmount) =>
            Math.Max(0, Subtotal - discountAmount) + ShippingFee;
    }

    public class CheckoutViewModel
    {
        public CartViewModel Cart { get; set; } = new();
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? PostalCode { get; set; }
        public string? Notes { get; set; }
        public string PaymentMethod { get; set; } = "Наложен платеж";
        public string? CouponCode { get; set; }
    }
}
