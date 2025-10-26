using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace Messaging.Email
{
    public class SmtpEmailSender : IEmailSender
    {
        private readonly IConfiguration _config;
        private readonly ILogger<SmtpEmailSender> _logger;
        public SmtpEmailSender(IConfiguration config, ILogger<SmtpEmailSender> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
        {
            // Read configuration with sane defaults for local/docker (MailHog)
            var host = (_config["Email:Smtp:Host"] ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(host)) host = "mailhog"; // default for docker dev

            var portStr = _config["Email:Smtp:Port"];
            var port = int.TryParse(portStr, out var parsedPort) ? parsedPort : (host.Equals("mailhog", StringComparison.OrdinalIgnoreCase) ? 1025 : 587);

            var user = (_config["Email:Smtp:User"] ?? string.Empty).Trim();
            var pass = (_config["Email:Smtp:Password"] ?? string.Empty).Trim();
            var from = (_config["Email:Smtp:From"] ?? user).Trim();
            if (string.IsNullOrWhiteSpace(from)) from = "noreply@example.com";

            var useStartTls = bool.TryParse(_config["Email:Smtp:UseStartTls"], out var tls)
                ? tls
                : !host.Equals("mailhog", StringComparison.OrdinalIgnoreCase);

            var toAddr = (to ?? string.Empty).Trim();
            var fromAddr = from;

            if (string.IsNullOrWhiteSpace(toAddr))
            {
                throw new ArgumentException("Recipient address is required", nameof(to));
            }

            _logger.LogInformation("SMTP send: host={Host}, port={Port}, from={From}, to={To}, tls={Tls}", host, port, fromAddr, toAddr, useStartTls);

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = useStartTls
            };
            if (!string.IsNullOrEmpty(user))
            {
                client.Credentials = new NetworkCredential(user, pass);
            }

            using var msg = new MailMessage(new MailAddress(fromAddr), new MailAddress(toAddr))
            {
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            await client.SendMailAsync(msg, cancellationToken);
            _logger.LogInformation("Sent email to {to}", toAddr);
        }
    }
}
