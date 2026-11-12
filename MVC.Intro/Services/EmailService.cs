using System.Net;
using System.Net.Mail;
using MVC.Intro.Models;

namespace MVC.Intro.Services
{
    public interface IEmailService
    {
        Task<bool> SendOrderConfirmationAsync(Order order);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task<bool> SendOrderConfirmationAsync(Order order)
        {
            var smtpHost = _config["Email:SmtpHost"];
            var smtpPortStr = _config["Email:SmtpPort"];
            var smtpUser = _config["Email:SmtpUser"];
            var smtpPass = _config["Email:SmtpPassword"];
            var fromEmail = _config["Email:FromAddress"] ?? smtpUser;
            var fromName = _config["Email:FromName"] ?? "Gerchev Fragrance";

            if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(smtpPass))
            {
                _logger.LogWarning("Email is not configured. Skipping order confirmation email for order {OrderNumber}.", order.OrderNumber);
                return false;
            }

            if (!int.TryParse(smtpPortStr, out int smtpPort)) smtpPort = 587;

            try
            {
                var body = BuildOrderConfirmationHtml(order);

                using var message = new MailMessage
                {
                    From = new MailAddress(fromEmail!, fromName),
                    Subject = $"✦ Поръчка потвърдена — {order.OrderNumber} | Gerchev Fragrance",
                    Body = body,
                    IsBodyHtml = true,
                };
                message.To.Add(order.CustomerEmail);

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUser, smtpPass),
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    Timeout = 15000
                };

                await client.SendMailAsync(message);
                _logger.LogInformation("Order confirmation email sent for {OrderNumber} to {Email}.", order.OrderNumber, order.CustomerEmail);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send order confirmation email for {OrderNumber} to {Email}.", order.OrderNumber, order.CustomerEmail);
                return false;
            }
        }

        private static string BuildOrderConfirmationHtml(Order order)
        {
            var itemsHtml = string.Join("\n", order.Items.Select(item => $@"
                <tr>
                    <td style=""padding:12px 16px; border-bottom:1px solid #2a2a2a;"">
                        <div style=""font-weight:600; color:#f0ead6; font-size:0.95rem;"">{System.Net.WebUtility.HtmlEncode(item.ProductName)}</div>
                        {(string.IsNullOrEmpty(item.SelectedSize) ? "" : $@"<div style=""color:#888; font-size:0.82rem; margin-top:3px;"">{System.Net.WebUtility.HtmlEncode(item.SelectedSize)}</div>")}
                    </td>
                    <td style=""padding:12px 16px; border-bottom:1px solid #2a2a2a; text-align:center; color:#aaa;"">{item.Quantity}</td>
                    <td style=""padding:12px 16px; border-bottom:1px solid #2a2a2a; text-align:right; color:#C9A84C; font-weight:600;"">{item.UnitPrice:F2} €</td>
                    <td style=""padding:12px 16px; border-bottom:1px solid #2a2a2a; text-align:right; color:#f0ead6; font-weight:700;"">{item.LineTotal:F2} €</td>
                </tr>"));

            var discountRow = order.DiscountAmount > 0
                ? $@"<tr>
                    <td colspan=""3"" style=""padding:8px 16px; color:#85e89d; font-size:0.88rem;"">Отстъпка ({order.AppliedCouponCode})</td>
                    <td style=""padding:8px 16px; text-align:right; color:#85e89d;"">-{order.DiscountAmount:F2} €</td>
                   </tr>"
                : "";

            var shippingRow = order.ShippingFee > 0
                ? $@"<tr>
                    <td colspan=""3"" style=""padding:8px 16px; color:#aaa; font-size:0.88rem;"">Доставка</td>
                    <td style=""padding:8px 16px; text-align:right; color:#aaa;"">{order.ShippingFee:F2} €</td>
                   </tr>"
                : @"<tr>
                    <td colspan=""3"" style=""padding:8px 16px; color:#85e89d; font-size:0.88rem;"">Доставка</td>
                    <td style=""padding:8px 16px; text-align:right; color:#85e89d;"">Безплатна</td>
                   </tr>";

            var orderDateLocal = order.OrderDate.ToLocalTime();

            return $@"<!DOCTYPE html>
<html lang=""bg"">
<head>
<meta charset=""utf-8"">
<meta name=""viewport"" content=""width=device-width, initial-scale=1"">
<title>Потвърждение на поръчка — Gerchev Fragrance</title>
</head>
<body style=""margin:0; padding:0; background:#080808; font-family:'Helvetica Neue',Arial,sans-serif; color:#f0ead6;"">
<table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#080808; padding:40px 20px;"">
  <tr><td align=""center"">
    <table width=""600"" cellpadding=""0"" cellspacing=""0"" style=""max-width:600px; width:100%;"">

      <!-- HEADER -->
      <tr>
        <td style=""background:linear-gradient(135deg,#111007 0%,#1c1708 100%); padding:40px 40px 30px; text-align:center; border-bottom:2px solid #C9A84C;"">
          <div style=""font-family:'Georgia',serif; font-size:28px; letter-spacing:6px; color:#f5f2eb; font-weight:700; text-transform:uppercase;"">GERCHEV</div>
          <div style=""font-size:11px; letter-spacing:5px; color:#C9A84C; text-transform:uppercase; margin-top:4px;"">FRAGRANCE</div>
          <div style=""margin-top:24px; color:#C9A84C; font-size:36px;"">✓</div>
          <h1 style=""font-family:'Georgia',serif; color:#f0ead6; font-size:24px; margin:12px 0 8px; font-weight:600;"">Поръчката е потвърдена</h1>
          <p style=""color:#aaa; font-size:14px; margin:0;"">Благодарим ви за поверието! Поръчката ви беше приета успешно.</p>
        </td>
      </tr>

      <!-- ORDER INFO -->
      <tr>
        <td style=""background:#0d0d0d; padding:30px 40px; border-bottom:1px solid #1f1f1f;"">
          <table width=""100%"" cellpadding=""0"" cellspacing=""0"">
            <tr>
              <td style=""width:50%; padding-bottom:10px;"">
                <div style=""font-size:11px; letter-spacing:2px; color:#C9A84C; text-transform:uppercase; margin-bottom:5px;"">Номер на поръчка</div>
                <div style=""font-size:16px; color:#fff; font-weight:700; letter-spacing:1px;"">{System.Net.WebUtility.HtmlEncode(order.OrderNumber)}</div>
              </td>
              <td style=""width:50%; padding-bottom:10px; text-align:right;"">
                <div style=""font-size:11px; letter-spacing:2px; color:#C9A84C; text-transform:uppercase; margin-bottom:5px;"">Дата на поръчка</div>
                <div style=""font-size:14px; color:#ccc;"">{orderDateLocal:dd.MM.yyyy HH:mm}</div>
              </td>
            </tr>
            <tr>
              <td colspan=""2"" style=""padding-top:12px; border-top:1px solid #222;"">
                <div style=""font-size:11px; letter-spacing:2px; color:#C9A84C; text-transform:uppercase; margin-bottom:5px;"">Получател</div>
                <div style=""color:#eee;"">{System.Net.WebUtility.HtmlEncode(order.CustomerName)}</div>
                <div style=""color:#888; font-size:13px;"">{System.Net.WebUtility.HtmlEncode(order.CustomerEmail)} · {System.Net.WebUtility.HtmlEncode(order.CustomerPhone)}</div>
              </td>
            </tr>
            <tr>
              <td colspan=""2"" style=""padding-top:12px;"">
                <div style=""font-size:11px; letter-spacing:2px; color:#C9A84C; text-transform:uppercase; margin-bottom:5px;"">Адрес за доставка</div>
                <div style=""color:#eee;"">{System.Net.WebUtility.HtmlEncode(order.ShippingAddress)}</div>
                <div style=""color:#888; font-size:13px;"">{System.Net.WebUtility.HtmlEncode(order.City)}{(string.IsNullOrEmpty(order.PostalCode) ? "" : ", " + order.PostalCode)}</div>
              </td>
            </tr>
          </table>
        </td>
      </tr>

      <!-- ORDER ITEMS -->
      <tr>
        <td style=""background:#0a0a0a; padding:30px 40px;"">
          <div style=""font-size:11px; letter-spacing:2px; color:#C9A84C; text-transform:uppercase; margin-bottom:16px;"">Поръчани продукти</div>
          <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""border-collapse:collapse;"">
            <thead>
              <tr style=""background:#161616;"">
                <th style=""padding:10px 16px; text-align:left; font-size:11px; letter-spacing:1px; color:#888; text-transform:uppercase; font-weight:500;"">Продукт</th>
                <th style=""padding:10px 16px; text-align:center; font-size:11px; letter-spacing:1px; color:#888; text-transform:uppercase; font-weight:500;"">Бр.</th>
                <th style=""padding:10px 16px; text-align:right; font-size:11px; letter-spacing:1px; color:#888; text-transform:uppercase; font-weight:500;"">Единична цена</th>
                <th style=""padding:10px 16px; text-align:right; font-size:11px; letter-spacing:1px; color:#888; text-transform:uppercase; font-weight:500;"">Сума</th>
              </tr>
            </thead>
            <tbody>
              {itemsHtml}
            </tbody>
            <tfoot>
              <tr><td colspan=""4"" style=""padding:0;""><div style=""border-top:1px solid #333; margin:8px 0;""></div></td></tr>
              <tr>
                <td colspan=""3"" style=""padding:8px 16px; color:#aaa; font-size:0.88rem;"">Междинна сума</td>
                <td style=""padding:8px 16px; text-align:right; color:#eee;"">{order.Subtotal:F2} €</td>
              </tr>
              {discountRow}
              {shippingRow}
              <tr><td colspan=""4"" style=""padding:0;""><div style=""border-top:2px solid #C9A84C; margin:8px 0;""></div></td></tr>
              <tr>
                <td colspan=""3"" style=""padding:12px 16px; font-family:'Georgia',serif; font-size:17px; color:#f0ead6; font-weight:700;"">Крайна сума</td>
                <td style=""padding:12px 16px; text-align:right; font-family:'Georgia',serif; font-size:20px; color:#C9A84C; font-weight:700;"">{order.TotalAmount:F2} €</td>
              </tr>
            </tfoot>
          </table>

          <div style=""margin-top:20px; padding:14px 16px; background:#0f0f0f; border:1px solid #222; font-size:13px; color:#888;"">
            <span style=""color:#C9A84C;"">●</span> Начин на плащане: <strong style=""color:#ccc;"">{System.Net.WebUtility.HtmlEncode(order.PaymentMethod)}</strong>
          </div>
        </td>
      </tr>

      <!-- FOOTER -->
      <tr>
        <td style=""background:#0d0d0d; padding:30px 40px; border-top:1px solid #1f1f1f; text-align:center;"">
          <p style=""color:#C9A84C; font-size:16px; font-family:'Georgia',serif; margin:0 0 10px;"">Благодарим, че избрахте Gerchev Fragrance</p>
          <p style=""color:#666; font-size:12px; line-height:1.7; margin:0 0 16px;"">
            Всеки парфюм в нашия бутик е с гарантиран оригинален произход.<br>
            При въпроси: <a href=""mailto:boutique@gerchevfragrance.com"" style=""color:#C9A84C; text-decoration:none;"">boutique@gerchevfragrance.com</a>
          </p>
          <p style=""color:#444; font-size:11px; letter-spacing:2px; text-transform:uppercase; margin:0;"">GERCHEV FRAGRANCE · HAUTE PARFUMERIE · СОФИЯ</p>
        </td>
      </tr>

    </table>
  </td></tr>
</table>
</body>
</html>";
        }
    }
}
