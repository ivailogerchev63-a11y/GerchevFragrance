namespace MVC.Intro.Models
{
    public class DiscountCoupon
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Code { get; set; } = "GERCHEV10";
        public string Email { get; set; } = string.Empty;
        public decimal DiscountPercentage { get; set; } = 10.00m;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);
        public bool IsUsed { get; set; } = false;
        public DateTime? UsedAt { get; set; }
        public Guid? OrderId { get; set; }

        public bool IsValid => !IsUsed && ExpiresAt > DateTime.UtcNow;
    }
}
