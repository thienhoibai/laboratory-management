using System;
using System.Collections.Generic;

namespace TestOrder.Infrastructure.Models;

public partial class Comment
{
    public long CommentId { get; set; }

    public long? TestId { get; set; }

    public string? Comment1 { get; set; }

    public DateOnly? CommentDate { get; set; }

    public virtual Booking? Test { get; set; }
}
