using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BlogService.Infrastructure.Models;

public partial class Category
{
    public int CategoryId { get; set; }

    public string CategoryName { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime? CreatedDate { get; set; }

    [JsonIgnore]
    public virtual ICollection<BlogPost> BlogPosts { get; set; } = new List<BlogPost>();
}
