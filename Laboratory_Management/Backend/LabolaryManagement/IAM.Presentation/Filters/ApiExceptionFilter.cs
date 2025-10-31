using Common.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IAM.Presentation.Filters
{
    public class ApiException : Exception
    {
        public string Code { get; }
        public List<(string field, string code, string message)>? FieldErrors { get; }
        public ApiException(string code, string? message = null, List<(string field, string code, string message)>? fieldErrors = null) : base(message)
        {
            Code = code;
            FieldErrors = fieldErrors;
        }
    }

    public class ApiExceptionFilter : IExceptionFilter
    {
        public void OnException(ExceptionContext context)
        {
            var ex = context.Exception;
            string code = ErrorCodes.InternalServerError;
            string? detail = ex.Message;
            List<(string field, string code, string message)>? fieldErrors = null;

            if (ex is ApiException aex)
            {
                code = aex.Code;
                fieldErrors = aex.FieldErrors;
            }

            var def = ErrorCatalog.Get(code);
            var pd = new ProblemDetails
            {
                Type = def.Type,
                Title = def.Title,
                Status = def.Status,
                Detail = detail
            };
            pd.Extensions["code"] = def.Code;
            if (fieldErrors != null && fieldErrors.Count > 0)
            {
                pd.Extensions["errors"] = fieldErrors.Select(e => new { field = e.field, code = e.code, message = e.message }).ToArray();
            }

            context.Result = new ObjectResult(pd) { StatusCode = def.Status };
            context.HttpContext.Response.ContentType = "application/problem+json";
            context.ExceptionHandled = true;
        }
    }
}
