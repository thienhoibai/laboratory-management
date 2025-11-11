using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BlogService.Application.Enums;

namespace BlogService.Application.DTOs
{
    public class UpdateBlogStatusDTO
    {
        public BlogPostStatus Status { get; set; }  
    }
}
