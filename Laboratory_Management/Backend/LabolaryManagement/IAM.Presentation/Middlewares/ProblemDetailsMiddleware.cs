using System.Net;
using Common.Errors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace IAM.Presentation.Middlewares
{
    public class ProblemDetailsMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ProblemDetailsMiddleware> _logger;

        public ProblemDetailsMiddleware(RequestDelegate next, ILogger<ProblemDetailsMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled exception");
                await WriteProblem(context, ErrorCodes.InternalServerError, ex.Message);
            }
        }

        public static async Task WriteProblem(HttpContext context, string code, string? detail = null)
        {
            var def = ErrorCatalog.Get(code);
            var pd = new ProblemDetails
            {
                Type = def.Type,
                Title = def.Title,
                Status = def.Status,
                Detail = detail
            };
            pd.Extensions["code"] = def.Code;

            context.Response.ContentType = "application/problem+json";
            context.Response.StatusCode = def.Status;
            await context.Response.WriteAsJsonAsync(pd);
        }
    }
}
