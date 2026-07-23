using Common.Errors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Common.Web.Filters
{
    public class ApiException : Exception
    {
        public string Code { get; }

        /// <summary>
        /// Mo ta do noi nem truyen vao, null neu chi nem kem ma loi.
        /// Khong dung Message vi khi message null thi .NET tra ve chuoi mac dinh
        /// "Exception of type '...' was thrown." - vo nghia voi nguoi dung cuoi.
        /// </summary>
        public string? Detail { get; }

        public List<(string field, string code, string message)>? FieldErrors { get; }
        public ApiException(string code, string? message = null, List<(string field, string code, string message)>? fieldErrors = null) : base(message)
        {
            Code = code;
            Detail = message;
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
                detail = aex.Detail;
            }

            var def = ErrorCatalog.Get(code);
            var pd = new ProblemDetails
            {
                Type = def.Type,
                Title = def.Title,
                Status = def.Status,
                // Khong co mo ta rieng thi dung mo ta trong catalog thay vi chuoi mac dinh cua .NET
                Detail = string.IsNullOrWhiteSpace(detail) ? def.Title : detail
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
