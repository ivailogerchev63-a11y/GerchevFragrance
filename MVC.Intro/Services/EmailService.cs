using System.Net;
using System.Net.Mail;

namespace MVC.Intro.Services
{
    public class EmailSettings
    {
        public string SmtpHost       { get; set; } = "smtp.gmail.com";
        public int    SmtpPort       { get; set; } = 587;
        public bool   UseSsl         { get; set; } = false;
        public bool   UseStartTls    { get; set; } = true;
        public string SenderEmail    { get; set; } = string.Empty;
        public string SenderName     { get; set; } = "GerchevFragrance";
        public string Username       { get; set; } = string.Empty;
        public string Password       { get; set; } = string.Empty;
    }

    public interface IEmailService
    {
        Task<(bool Success, string? Error)> SendWelcomeCouponAsync(
            string toEmail, string couponCode, DateTime expiresAt);
    }

    public class EmailService : IEmailService
    {
        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;

        public EmailService(EmailSettings settings, ILogger<EmailService> logger)
        {
            _settings = settings;
            _logger   = logger;
        }

        public async Task<(bool Success, string? Error)> SendWelcomeCouponAsync(
            string toEmail, string couponCode, DateTime expiresAt)
        {
            // If no SMTP credentials are configured, skip silently in dev.
            if (string.IsNullOrWhiteSpace(_settings.Password) ||
                string.IsNullOrWhiteSpace(_settings.SenderEmail))
            {
                _logger.LogWarning(
                    "Email delivery skipped — SMTP credentials not configured. " +
                    "Set Email:SenderEmail and Email:Password in appsettings or User Secrets.");
                return (false, "Имейл доставката не е конфигурирана. " +
                               "Моля, свържете се с администратора или проверете вашия код ръчно.");
            }

            var subject  = "Добре дошли в GerchevFragrance — вашият 10% промо код";
            var expStr   = expiresAt.ToString("dd.MM.yyyy");
            var bodyHtml = BuildEmailHtml(toEmail, couponCode, expStr);

            try
            {
                using var message = new MailMessage();
                message.From       = new MailAddress(_settings.SenderEmail, _settings.SenderName);
                message.To.Add(toEmail);
                message.Subject    = subject;
                message.Body       = bodyHtml;
                message.IsBodyHtml = true;

                using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort);
                client.EnableSsl            = _settings.UseSsl;
                client.UseDefaultCredentials = false;
                client.Credentials          = new NetworkCredential(
                    _settings.Username, _settings.Password);

                if (_settings.UseStartTls)
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                await client.SendMailAsync(message);
                _logger.LogInformation("Welcome coupon email sent to {Email}", toEmail);
                return (true, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send coupon email to {Email}", toEmail);
                return (false,
                    "Имейлът не можа да бъде изпратен. " +
                    "Вашият код е валиден — запишете го сега: " + couponCode);
            }
        }

        private static string BuildEmailHtml(
            string email, string couponCode, string expiryDate) => $@"
<!DOCTYPE html>
<html lang=""bg"">
<head>
<meta charset=""utf-8"">
<meta name=""viewport"" content=""width=device-width,initial-scale=1"">
<title>Добре дошли в GerchevFragrance</title>
</head>
<body style=""margin:0;padding:0;background:#0d0d0d;font-family:'Georgia',serif;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#0d0d0d;"">
    <tr><td align=""center"" style=""padding:40px 20px;"">

      <table width=""600"" cellpadding=""0"" cellspacing=""0""
             style=""max-width:600px;background:#141414;border:1px solid #2a2a2a;"">

        <!-- Header -->
        <tr>
          <td align=""center"" style=""padding:48px 40px 32px;
              border-bottom:1px solid #2a2a2a;"">
            <div style=""font-family:'Georgia',serif;font-size:28px;
                color:#C9A84C;letter-spacing:4px;text-transform:uppercase;"">
              ✦ GerchevFragrance
            </div>
            <div style=""height:1px;background:linear-gradient(to right,transparent,#C9A84C,transparent);
                margin:20px auto;width:180px;""></div>
            <p style=""color:#888;font-size:12px;letter-spacing:3px;
                text-transform:uppercase;margin:0;"">
              Луксозни Парфюми
            </p>
          </td>
        </tr>

        <!-- Body -->
        <tr>
          <td style=""padding:40px 48px;"">
            <h2 style=""font-family:'Georgia',serif;color:#f0ead6;
                font-size:26px;margin:0 0 16px;"">
              Добре дошли!
            </h2>
            <p style=""color:#999;font-size:15px;line-height:1.8;margin:0 0 28px;"">
              Благодарим ви за абонамента за бюлетина на GerchevFragrance.
              Като знак на нашата признателност, приготвихме за вас
              ексклузивен 10% отстъпка код за вашата първа поръчка.
            </p>

            <!-- Coupon box -->
            <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
              <tr>
                <td align=""center"" style=""background:#1a1a1a;border:1px solid #C9A84C;
                    padding:32px;margin:0 0 28px;"">
                  <p style=""color:#888;font-size:11px;letter-spacing:3px;
                      text-transform:uppercase;margin:0 0 12px;"">
                    Вашият ексклузивен код
                  </p>
                  <div style=""font-family:'Courier New',monospace;font-size:28px;
                      color:#C9A84C;letter-spacing:6px;font-weight:bold;"">
                    {couponCode}
                  </div>
                  <p style=""color:#555;font-size:12px;margin:12px 0 0;"">
                    10% отстъпка · Валиден до {expiryDate}
                  </p>
                </td>
              </tr>
            </table>

            <h3 style=""font-family:'Georgia',serif;color:#f0ead6;
                font-size:16px;margin:28px 0 12px;"">
              Как да използвате кода?
            </h3>
            <ol style=""color:#999;font-size:14px;line-height:2;padding-left:20px;"">
              <li>Добавете парфюм/и в количката</li>
              <li>Отидете на страницата за плащане</li>
              <li>Въведете кода <strong style=""color:#C9A84C;"">{couponCode}</strong> в полето за промо код</li>
              <li>Натиснете <em>Приложи</em> — отстъпката се изчислява автоматично</li>
              <li>Потвърдете поръчката</li>
            </ol>

            <div style=""height:1px;background:linear-gradient(to right,transparent,#333,transparent);
                margin:32px 0;""></div>

            <p style=""color:#555;font-size:12px;line-height:1.8;"">
              Кодът е валиден до <strong style=""color:#888;"">{expiryDate}</strong> и може да се
              използва само веднъж. Ако имате въпроси, свържете се с нас.
            </p>
          </td>
        </tr>

        <!-- Footer -->
        <tr>
          <td align=""center""
              style=""padding:24px;border-top:1px solid #1e1e1e;
                     background:#111;"">
            <p style=""color:#444;font-size:11px;margin:0;"">
              © 2026 GerchevFragrance. Всички права запазени.
            </p>
          </td>
        </tr>

      </table>
    </td></tr>
  </table>
</body>
</html>";
    }
}
