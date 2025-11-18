using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Messaging.Email
{
    public interface IEmailTemplateRenderer
    {
        Task<string> RenderAsync(string templateName, IDictionary<string, string> model, CancellationToken ct = default);
    }
}
