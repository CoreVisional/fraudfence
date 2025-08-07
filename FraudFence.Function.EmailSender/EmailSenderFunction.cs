using Amazon.Lambda.Core;
using Amazon.Lambda.SQSEvents;
using System.Net;
using System.Net.Mail;
using System.Text.Json;
using System.Web;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace FraudFence.Function.EmailSender;

public class EmailSenderFunction
{
    private readonly string _smtpHost;
    private readonly int _smtpPort;
    private readonly string _smtpUsername;
    private readonly string _smtpPassword;
    private readonly string _senderEmail;
    private readonly bool _enableSsl;

    public EmailSenderFunction()
    {
        _smtpHost = Environment.GetEnvironmentVariable("SMTP_HOST")!;
        _smtpPort = int.Parse(Environment.GetEnvironmentVariable("SMTP_PORT")!);
        _smtpUsername = Environment.GetEnvironmentVariable("SMTP_USERNAME")!;
        _smtpPassword = Environment.GetEnvironmentVariable("SMTP_PASSWORD")!;
        _senderEmail = Environment.GetEnvironmentVariable("SENDER_EMAIL")!;
        _enableSsl = bool.Parse(Environment.GetEnvironmentVariable("ENABLE_SSL") ?? "true");
    }

    public async Task FunctionHandler(SQSEvent evt, ILambdaContext context)
    {
        foreach (var record in evt.Records)
        {
            context.Logger.LogInformation($"Processing SQS message: {record.MessageId}");

            var emailData = JsonDocument.Parse(record.Body);
            var to = emailData.RootElement.GetProperty("To").GetString()!;
            var subject = emailData.RootElement.GetProperty("Subject").GetString()!;
            var body = emailData.RootElement.GetProperty("Body").GetString()!;

            body = HttpUtility.HtmlDecode(body);

            await SendEmail(to, subject, body, context);

            context.Logger.LogInformation($"Email sent to {to}");
        }
    }

    private async Task SendEmail(string to, string subject, string htmlBody, ILambdaContext context)
    {
        var addr = (to ?? string.Empty).Trim();

        if (string.IsNullOrWhiteSpace(addr))
        {
            context.Logger.LogError("Email address is empty or whitespace");
            return;
        }

        if (!MailAddress.TryCreate(addr, out var parsed))
        {
            context.Logger.LogError($"Invalid email address: {to}");
            return;
        }

        using var smtp = new SmtpClient(_smtpHost, _smtpPort)
        {
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(_smtpUsername, _smtpPassword),
            EnableSsl = _enableSsl
        };

        using var message = new MailMessage
        {
            From = new MailAddress(_senderEmail),
            Subject = subject,
            Body = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(parsed);

        await smtp.SendMailAsync(message);
        context.Logger.LogInformation($"Email sent via SMTP to {parsed.Address}");
    }
}
