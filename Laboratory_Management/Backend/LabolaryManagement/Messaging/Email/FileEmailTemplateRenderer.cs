using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Reflection;
using System.Text;

namespace Messaging.Email
{
    public class FileEmailTemplateRenderer : IEmailTemplateRenderer
    {
        private readonly ILogger<FileEmailTemplateRenderer> _logger;
        private readonly ConcurrentDictionary<string, string> _cache = new();
        private readonly Assembly _assembly;
        private readonly ManifestEmbeddedFileProvider _embedded;
        private readonly string _fsTemplatesRoot;

        public FileEmailTemplateRenderer(ILogger<FileEmailTemplateRenderer> logger)
        {
            _logger = logger;
            _assembly = typeof(FileEmailTemplateRenderer).Assembly;
            _embedded = new ManifestEmbeddedFileProvider(_assembly);
            _fsTemplatesRoot = Path.Combine(AppContext.BaseDirectory, "Templates", "Emails");
        }

        public Task<string> RenderAsync(string templateName, IDictionary<string, string> model, CancellationToken ct = default)
        {
            var embeddedPath = $"Templates/Emails/{templateName}.html";

            string raw;
            // Try embedded first
            var fileInfo = _embedded.GetFileInfo(embeddedPath);
            if (fileInfo.Exists)
            {
                if (!_cache.TryGetValue(embeddedPath, out raw!))
                {
                    using var stream = fileInfo.CreateReadStream();
                    using var reader = new StreamReader(stream, Encoding.UTF8);
                    raw = reader.ReadToEnd();
                    _cache[embeddedPath] = raw;
                }
            }
            else
            {
                // Fallback to file system
                var fsPath = Path.Combine(_fsTemplatesRoot, templateName + ".html");
                if (!File.Exists(fsPath))
                {
                    _logger.LogError("Email template not found in resources or file system: {Template}", templateName);
                    throw new FileNotFoundException($"Email template not found: {templateName}");
                }
                raw = File.ReadAllText(fsPath, Encoding.UTF8);
            }

            var html = raw;
            foreach (var kv in model)
            {
                html = html.Replace("{{" + kv.Key + "}}", kv.Value);
            }
            return Task.FromResult(html);
        }
    }
}
