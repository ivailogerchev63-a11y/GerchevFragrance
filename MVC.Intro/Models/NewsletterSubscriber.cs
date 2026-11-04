using System.ComponentModel.DataAnnotations;

namespace MVC.Intro.Models
{
    /// <summary>
    /// Represents one newsletter subscriber and their associated welcome coupon.
    /// </summary>
    public class NewsletterSubscriber
    {
        public Guid Id { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(256)]
        public required string Email { get; set; }

        public DateTime SubscribedAt { get; set; } = DateTime.UtcNow;

        // ── Coupon fields ────────────────────────────────────────────────────────

        /// <summary>e.g. "GERCH-A3X9F2"</summary>
        [StringLength(30)]
        public string? CouponCode { get; set; }

        /// <summary>Percentage discount, e.g. 10 = 10 %.</summary>
        public int DiscountPercent { get; set; } = 10;

        public DateTime? ExpiresAt { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime? UsedAt { get; set; }

        /// <summary>Free-text order reference written at order completion.</summary>
        [StringLength(100)]
        public string? UsedForOrderRef { get; set; }
    }
}
