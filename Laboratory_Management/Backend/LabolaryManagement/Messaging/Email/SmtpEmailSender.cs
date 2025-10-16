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
            var host = _config["Email:Smtp:Host"] ?? "";
            var port = int.TryParse(_config["Email:Smtp:Port"], out var p) ? p : 587;
            var user = _config["Email:Smtp:User"] ?? "";
            var pass = _config["Email:Smtp:Password"] ?? "";
            var from = _config["Email:Smtp:From"] ?? user;
            var useStartTls = bool.TryParse(_config["Email:Smtp:UseStartTls"], out var tls) ? tls : true;

            using var client = new SmtpClient(host, port)
            {
                EnableSsl = useStartTls,
                Credentials = new NetworkCredential(user, pass)
            };
            using var msg = new MailMessage(new MailAddress(from), new MailAddress(to))
            {
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            await client.SendMailAsync(msg, cancellationToken);
            _logger.LogInformation("Sent email to {to}", to);
        }
    }
}
