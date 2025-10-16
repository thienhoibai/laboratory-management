using Common.Pagination;
using Common.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using System.Web;

namespace Common.Web.Filters
{
    public class ResponseWrapperFilter : IAsyncResultFilter
    {
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            if (context.Result is ObjectResult obj && obj.Value != null)
            {
                object? data = obj.Value;
                object? meta = null;

                var type = data.GetType();
                if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(PageResult<>))
                {
                    var pageProp = type.GetProperty("Page");
                    var pageSizeProp = type.GetProperty("PageSize");
                    var totalProp = type.GetProperty("Total");
                    var totalPagesProp = type.GetProperty("TotalPages");
                    var itemsProp = type.GetProperty("Items");

                    var page = (int)(pageProp?.GetValue(data) ?? 1);
                    var pageSize = (int)(pageSizeProp?.GetValue(data) ?? 0);
                    var total = (long)(totalProp?.GetValue(data) ?? 0L);
                    var totalPages = (int)(totalPagesProp?.GetValue(data) ?? 0);

                    meta = new PageMeta
                    {
                        Page = page,
                        PageSize = pageSize,
                        TotalItems = total,
                        TotalPages = totalPages
                    };
                    data = itemsProp?.GetValue(data);

                    var headers = context.HttpContext.Response.Headers;
                    headers["X-Total-Count"] = new StringValues(total.ToString());
                    var request = context.HttpContext.Request;
                    string BuildLink(int targetPage)
                    {
                        var qb = HttpUtility.ParseQueryString(request.QueryString.Value ?? string.Empty);
                        qb.Set("page", targetPage.ToString());
                        qb.Set("pageSize", pageSize.ToString());
                        var qs = qb.ToString();
                        return $"{request.Scheme}://{request.Host}{request.Path}?{qs}";
                    }
                    var links = new List<string>();
                    if (page > 1) links.Add($"<${BuildLink(page - 1)}>; rel=\"prev\"");
                    if (page < totalPages) links.Add($"<${BuildLink(page + 1)}>; rel=\"next\"");
                    if (links.Count > 0) headers["Link"] = new StringValues(string.Join(", ", links));
                }

                var traceId = context.HttpContext.TraceIdentifier;
                var envelope = ApiResponse.Success(data, meta, traceId);

                if (obj is CreatedAtActionResult created)
                {
                    created.Value = envelope;
                }
                else
                {
                    context.Result = new ObjectResult(envelope)
                    {
                        StatusCode = obj.StatusCode ?? 200,
                        DeclaredType = typeof(ApiResponse)
                    };
                }

                context.HttpContext.Response.ContentType = "application/json; charset=utf-8";
            }

            await next();
        }
    }
}
