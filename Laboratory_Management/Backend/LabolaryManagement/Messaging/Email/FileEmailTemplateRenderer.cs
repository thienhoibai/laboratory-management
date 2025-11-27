using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Globalization;
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
            // Resolve template with culture fallbacks
            var culture = CultureInfo.CurrentUICulture;
            
            // ✅ Ưu tiên tìm theo culture cụ thể trước (vi-VN, en-US, ...)
            var candidates = new List<string>
            {
                $"Templates/Emails/{templateName}.{culture.Name}.html",                      // vi-VN
                $"Templates/Emails/{templateName}.{culture.TwoLetterISOLanguageName}.html",  // vi
                $"Templates/Emails/{templateName}.html",                                     // fallback no culture
                $"Templates/Emails/{templateName}.en-US.html"                                // fallback en-US
            };

            string? raw = null;

            foreach (var embeddedPath in candidates)
            {
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
                    break;
                }

                // Try file system mirror path
                var fsPath = Path.Combine(_fsTemplatesRoot, Path.GetFileName(embeddedPath));
                if (File.Exists(fsPath))
                {
                    if (!_cache.TryGetValue(fsPath, out raw!))
                    {
                        raw = File.ReadAllText(fsPath, Encoding.UTF8);
                        _cache[fsPath] = raw;
                    }
                    break;
                }
            }

            if (raw == null)
            {
                _logger.LogError("Email template not found (tried: {Candidates})", string.Join(", ", candidates));
                throw new FileNotFoundException($"Email template not found: {templateName}");
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
