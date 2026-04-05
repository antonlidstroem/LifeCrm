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

        public async Task SendAsync(
            string  toEmail,
            string  toName,
            string  subject,
            string  htmlBody,
            IEnumerable<EmailAttachment>? attachments = null,
            CancellationToken ct = default)
        {
            var section = _config.GetSection("Email");
            bool dryRun = bool.TryParse(section["DryRun"], out var b) && b;

            if (dryRun)
            {
                _logger.LogInformation(
                    "[Email DryRun] To={To} Subject={Subject} Attachments={Count}",
                    toEmail, subject, attachments?.Count() ?? 0);
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(section["FromName"] ?? "LifeCrm", section["FromEmail"]));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder { HtmlBody = htmlBody };

            if (attachments is not null)
            {
                foreach (var att in attachments)
                {
                    builder.Attachments.Add(
                        att.FileName,
                        att.Bytes,
                        ContentType.Parse(att.ContentType));
                }
            }

            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(
                section["Host"],
                int.Parse(section["Port"] ?? "587"),
                SecureSocketOptions.StartTls, ct);
            await client.AuthenticateAsync(section["Username"], section["Password"], ct);
            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);
        }
    }
}
