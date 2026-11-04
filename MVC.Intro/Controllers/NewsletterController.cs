using Microsoft.AspNetCore.Mvc;
using MVC.Intro.Services;

namespace MVC.Intro.Controllers
{
    public class NewsletterController : Controller
    {
        private readonly CouponService  _coupon;
        private readonly IEmailService  _email;

        public NewsletterController(CouponService coupon, IEmailService email)
        {
            _coupon = coupon;
            _email  = email;
        }

        // POST /Newsletter/Subscribe
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Subscribe([FromForm] string subscriberEmail)
        {
            if (string.IsNullOrWhiteSpace(subscriberEmail) ||
                !subscriberEmail.Contains('@'))
            {
                return Json(new { success = false, message = "Моля, въведете валиден имейл адрес." });
            }

            var sub = await _coupon.CreateSubscriptionAsync(subscriberEmail);

            if (sub == null)
            {
                // Already subscribed — do NOT reveal whether the code was used/expired,
                // just give a friendly message.
                return Json(new
                {
                    success   = false,
                    duplicate = true,
                    message   = "Този имейл вече е абониран. Проверете входящата си поща за кода."
                });
            }

            // Try to send email — but never lie about it
            var (emailSent, emailError) = await _email.SendWelcomeCouponAsync(
                sub.Email,
                sub.CouponCode!,
                sub.ExpiresAt!.Value);

            return Json(new
            {
                success    = true,
                couponCode = sub.CouponCode,
                expiresAt  = sub.ExpiresAt!.Value.ToString("dd.MM.yyyy"),
                emailSent,
                emailNote  = emailSent
                    ? "Кодът беше изпратен и на вашия имейл."
                    : emailError
            });
        }

        // POST /Newsletter/ValidateCoupon  — AJAX endpoint used by checkout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ValidateCoupon([FromForm] string couponCode)
        {
            if (string.IsNullOrWhiteSpace(couponCode))
                return Json(new { valid = false, message = "Въведете промо код." });

            var result = await _coupon.ValidateAsync(couponCode);
            return Json(new
            {
                valid           = result.IsValid,
                message         = result.Message,
                discountPercent = result.DiscountPercent
            });
        }
    }
}
