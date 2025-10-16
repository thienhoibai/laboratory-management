using System.Threading;
using System.Threading.Tasks;

namespace Messaging.Email
{
    public interface IEmailSender
    {
        Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default);
    }
}
