using System;
using System.Collections.Generic;

namespace BlogService.Infrastructure.Models;

public partial class BlogPostTag
{
    public int? PostId { get; set; }

    public int? TagId { get; set; }

    public virtual BlogPost? Post { get; set; }

    public virtual Tag? Tag { get; set; }
}
