using FraudFence.Interface.Common;
using System.Net;
using System.Net.Mail;

namespace FraudFence.Service.Common
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(EmailSettings emailSettings)
        {
            _emailSettings = emailSettings;
        }

        async Task IEmailService.SendEmail(string subject, string[] sendTo, string body)
        {
            using var smtp = new SmtpClient(_emailSettings.Host, _emailSettings.Port)
            {
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(_emailSettings.Username, _emailSettings.Password),
                EnableSsl = _emailSettings.UseSsl,
            };

            foreach (var raw in sendTo.Distinct())
            {
                var addr = (raw ?? string.Empty).Trim();

                if (string.IsNullOrWhiteSpace(addr)) continue;

                if (!MailAddress.TryCreate(addr, out var parsed))
                    throw new FormatException($"Invalid email '{raw}'");

                using var mail = new MailMessage
                {
                    From = new MailAddress(_emailSettings.SenderEmailAddress),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                mail.To.Add(parsed);

                await smtp.SendMailAsync(mail);
            }
        }
    }
}
