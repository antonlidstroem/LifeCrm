using LifeCrm.Core.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace LifeCrm.Infrastructure.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration config, ILogger<EmailService> logger)
        { _config = config; _logger = logger; }

        public async Task SendAsync(string toEmail, string toName, string subject,
            string htmlBody, byte[]? attachmentBytes = null, string? attachmentName = null,
            CancellationToken ct = default)
        {
            var section = _config.GetSection("Email");
            bool dryRun = bool.TryParse(section["DryRun"], out var b) && b;

            if (dryRun)
            {
                _logger.LogInformation("[Email DryRun] To={To} Subject={Subject}", toEmail, subject);
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(section["FromName"] ?? "LifeCrm", section["FromEmail"]));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };
            if (attachmentBytes is not null && !string.IsNullOrEmpty(attachmentName))
                builder.Attachments.Add(attachmentName, attachmentBytes, ContentType.Parse("application/pdf"));

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(section["Host"], int.Parse(section["Port"] ?? "587"),
                SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(section["Username"], section["Password"], ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
    }
}
