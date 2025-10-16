using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common.Pagination
{
    // Generic pagination result without EF Core dependency
    public class PageResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
        public int Page { get; init; }
        public int PageSize { get; init; }
        public long Total { get; init; }
        public int TotalPages => PageSize <= 0 ? 0 : (int)Math.Ceiling((double)Total / PageSize);

        public static PageResult<T> From(IEnumerable<T> items, int page, int pageSize, long total)
            => new PageResult<T>
            {
                Items = items is IReadOnlyList<T> r ? r : items.ToList(),
                Page = page,
                PageSize = pageSize,
                Total = total
            };

        public static PageResult<T> Empty(int page, int pageSize) => new PageResult<T>
        {
            Items = Array.Empty<T>(),
            Page = page,
            PageSize = pageSize,
            Total = 0
        };
    }
}
