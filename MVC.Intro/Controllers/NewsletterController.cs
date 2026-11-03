using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MVC.Intro.Data;
using MVC.Intro.Models;

namespace MVC.Intro.Controllers
{
    public class NewsletterController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<NewsletterController> _logger;

        public NewsletterController(AppDbContext context, ILogger<NewsletterController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> Subscribe(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@') || !email.Contains('.'))
            {
                return Json(new { success = false, message = "Моля, въведете валиден имейл адрес." });
            }

            var normalizedEmail = email.Trim().ToLowerInvariant();

            // Check if this email already received a coupon
            var existingCoupon = await _context.DiscountCoupons
                .FirstOrDefaultAsync(c => c.Email.ToLower() == normalizedEmail && c.Code == "GERCHEV10");

            if (existingCoupon != null)
            {
                if (existingCoupon.IsUsed)
                {
                    return Json(new
                    {
                        success = false,
                        alreadyUsed = true,
                        message = "Този имейл адрес вече е използвал своя персонален код за добре дошли GERCHEV10."
                    });
                }

                if (existingCoupon.ExpiresAt < DateTime.UtcNow)
                {
                    return Json(new
                    {
                        success = false,
                        expired = true,
                        message = "Вашият код GERCHEV10 за този имейл е изтекъл (валидността е 7 дни от регистрацията)."
                    });
                }

                // Still valid, remind user
                var daysLeft = (int)Math.Ceiling((existingCoupon.ExpiresAt - DateTime.UtcNow).TotalDays);
                return Json(new
                {
                    success = true,
                    code = "GERCHEV10",
                    isExisting = true,
                    discount = 10,
                    expiresAt = existingCoupon.ExpiresAt.ToString("dd.MM.yyyy"),
                    message = $"Вече сте абонирани! Вашият персонален код е GERCHEV10 (10% отстъпка, валиден още {daysLeft} дни)."
                });
            }

            // Create new individual coupon record for this subscriber
            var newCoupon = new DiscountCoupon
            {
                Id = Guid.NewGuid(),
                Code = "GERCHEV10",
                Email = normalizedEmail,
                DiscountPercentage = 10.00m,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                IsUsed = false
            };

            _context.DiscountCoupons.Add(newCoupon);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Generated welcome coupon GERCHEV10 for subscriber {Email}, expires on {ExpiresAt}", normalizedEmail, newCoupon.ExpiresAt);

            return Json(new
            {
                success = true,
                code = "GERCHEV10",
                discount = 10,
                expiresAt = newCoupon.ExpiresAt.ToString("dd.MM.yyyy"),
                message = "Благодарим за абонамента! Вашият ексклузивен код за 10% отстъпка е GERCHEV10. Валиден е 7 дни."
            });
        }
    }
}
