using System;

namespace Common.Responses
{
    public class ApiResponse
    {
        public string Code { get; set; } = "OK";
        public string Message { get; set; } = "Thành công";
        public object? Data { get; set; }
        public object? Meta { get; set; }
        public string TraceId { get; set; } = string.Empty;
        public string Timestamp { get; set; } = DateTime.UtcNow.ToString("O");

        public static ApiResponse Success(object? data, object? meta, string traceId)
            => new ApiResponse
            {
                Code = "OK",
                Message = "Thành công",
                Data = data,
                Meta = meta,
                TraceId = traceId,
                Timestamp = DateTime.UtcNow.ToString("O")
            };
    }

    public class PageMeta
    {
        public int Page { get; set; }
        public int PageSize { get; set; }
        public long TotalItems { get; set; }
        public int TotalPages { get; set; }
    }
}
