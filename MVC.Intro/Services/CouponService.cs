using Microsoft.EntityFrameworkCore;
using MVC.Intro.Data;
using MVC.Intro.Models;
using System.Security.Cryptography;
using System.Text;

namespace MVC.Intro.Services
{
    public class CouponValidationResult
    {
        public bool   IsValid          { get; init; }
        public string Message          { get; init; } = string.Empty;
        public int    DiscountPercent  { get; init; }
        public NewsletterSubscriber? Subscriber { get; init; }
    }

    public class CouponService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<CouponService> _logger;

        public CouponService(AppDbContext db, ILogger<CouponService> logger)
        {
            _db     = db;
            _logger = logger;
        }

        // ── Generate ──────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a subscriber record with a unique coupon code.
        /// Returns null if the email is already subscribed.
        /// </summary>
        public async Task<NewsletterSubscriber?> CreateSubscriptionAsync(string email)
        {
            email = email.Trim().ToLowerInvariant();

            // Duplicate guard
            var existing = await _db.NewsletterSubscribers
                .FirstOrDefaultAsync(s => s.Email == email);
            if (existing != null) return null;

            var code = GenerateCode();
            var sub  = new NewsletterSubscriber
            {
                Id              = Guid.NewGuid(),
                Email           = email,
                SubscribedAt    = DateTime.UtcNow,
                CouponCode      = code,
                DiscountPercent = 10,
                ExpiresAt       = DateTime.UtcNow.AddDays(7),
                IsUsed          = false
            };

            _db.NewsletterSubscribers.Add(sub);
            await _db.SaveChangesAsync();
            return sub;
        }

        // ── Validate ──────────────────────────────────────────────────────────────

        public async Task<CouponValidationResult> ValidateAsync(string rawCode)
        {
            var code = rawCode.Trim().ToUpperInvariant();

            var sub = await _db.NewsletterSubscribers
                .FirstOrDefaultAsync(s => s.CouponCode == code);

            if (sub == null)
                return Fail("Промо кодът не е валиден.");

            if (sub.IsUsed)
                return Fail("Промо кодът вече е използван.");

            if (sub.ExpiresAt.HasValue && sub.ExpiresAt.Value < DateTime.UtcNow)
                return Fail("Промо кодът е изтекъл.");

            return new CouponValidationResult
            {
                IsValid         = true,
                Message         = $"Кодът е приложен — {sub.DiscountPercent}% отстъпка!",
                DiscountPercent = sub.DiscountPercent,
                Subscriber      = sub
            };
        }

        // ── Mark used ─────────────────────────────────────────────────────────────

        /// <summary>
        /// Atomically marks the coupon as used. Returns false if already used
        /// (safe against concurrent checkout attempts).
        /// </summary>
        public async Task<bool> MarkUsedAsync(string rawCode, string orderRef)
        {
            var code = rawCode.Trim().ToUpperInvariant();

            // Load with concurrency-safe check
            var sub = await _db.NewsletterSubscribers
                .FirstOrDefaultAsync(s => s.CouponCode == code && !s.IsUsed);

            if (sub == null) return false;

            sub.IsUsed          = true;
            sub.UsedAt          = DateTime.UtcNow;
            sub.UsedForOrderRef = orderRef;

            try
            {
                await _db.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Concurrency conflict marking coupon {Code} used.", code);
                return false;
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────────

        private static string GenerateCode()
        {
            // Format: GERCH-XXXXXX  (6 uppercase alphanumeric chars)
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789"; // no O,0,I,1 ambiguity
            var sb = new StringBuilder("GERCH-");
            using var rng = RandomNumberGenerator.Create();
            var buf = new byte[6];
            rng.GetBytes(buf);
            foreach (var b in buf)
                sb.Append(chars[b % chars.Length]);
            return sb.ToString();
        }

        private static CouponValidationResult Fail(string msg) =>
            new() { IsValid = false, Message = msg };
    }
}
