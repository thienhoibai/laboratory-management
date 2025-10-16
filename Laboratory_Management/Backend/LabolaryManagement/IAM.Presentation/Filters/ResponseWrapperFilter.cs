using Common.Pagination;
using Common.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Primitives;
using Microsoft.AspNetCore.WebUtilities;

namespace IAM.Presentation.Filters
{
    // Wrap successful responses into the unified envelope
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
                        var uri = new UriBuilder
                        {
                            Scheme = request.Scheme,
                            Host = request.Host.Host,
                            Path = request.Path,
                            Port = request.Host.Port ?? -1
                        };
                        var queryDict = new Dictionary<string, string?>
                        {
                            ["page"] = targetPage.ToString(),
                            ["pageSize"] = pageSize.ToString()
                        };
                        uri.Query = QueryHelpers.AddQueryString(string.Empty, queryDict).TrimStart('?');
                        return uri.Uri.ToString();
                    }
                    var links = new List<string>();
                    if (page > 1) links.Add($"<{BuildLink(page - 1)}>; rel=\"prev\"");
                    if (page < totalPages) links.Add($"<{BuildLink(page + 1)}>; rel=\"next\"");
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
            }

            await next();
        }
    }
}
