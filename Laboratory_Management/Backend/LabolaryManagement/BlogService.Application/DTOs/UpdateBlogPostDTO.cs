using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogService.Application.DTOs
{
    public class UpdateBlogPostDTO
    {
        
        public string? Title { get; set; }
        public string? Content { get; set; }
        public int? CategoryId { get; set; }
        public int ? AuthorId { get; set; }

        public string ? ThumbnailUrl { get; set; }
        public DateTime? UpdatedDate { get; set; }

        
    }
}
