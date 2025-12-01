using System;
using System.Collections.Generic;

namespace BlogService.Infrastructure.Models;

public partial class Comment
{
    public int CommentId { get; set; }

    public int? PostId { get; set; }

    public Guid? UserId { get; set; }

    public string? Content { get; set; }

    public DateTime? CreatedDate { get; set; }

    public bool? IsUpdated { get; set; }

    public virtual BlogPost? Post { get; set; }
}
