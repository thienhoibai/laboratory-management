using System;
using System.Collections.Generic;

namespace BlogService.Infrastructure.Models;

public partial class Tag
{
    public int TagId { get; set; }

    public string? TagName { get; set; }

    public virtual ICollection<BlogPost> Posts { get; set; } = new List<BlogPost>();
}
