using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BlogService.Application.DTOs
{
    public class CommentDTO

    {
        public int? CommentId { get; set; } 

        public int? PostId { get; set; }
        
        public Guid? UserId { get; set; }

        public string? Content { get; set; }

        public DateTime? CreatedDate { get; set; }

        public bool? IsUpdated { get; set; }

    }
}
