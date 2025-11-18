using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogService.Application.DTOs
{
    public class BlogPostCreateDTO
    {

        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public Guid? AuthorId { get; set; }
        public int CategoryId { get; set; }
        public bool? IsPublished { get; set; }
        public string? ThumbnailUrl { get; set; }
    }
}
