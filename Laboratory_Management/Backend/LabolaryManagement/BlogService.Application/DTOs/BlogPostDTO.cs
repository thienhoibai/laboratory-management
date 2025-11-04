using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogService.Application.DTOs
{
    public class BlogPostDTO
    {
        
        public string? Title { get; set; }
        public string? Content { get; set; }
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public DateTime? CreatedDate { get; set; }
        public DateTime? UpdatedDate { get; set; }
        public bool? IsPublished { get; set; }
        public bool? IsApproved { get; set; }
        public string? ThumbnailUrl { get; set; }
    }
}
