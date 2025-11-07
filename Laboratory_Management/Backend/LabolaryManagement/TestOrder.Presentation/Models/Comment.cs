using System;
using System.Collections.Generic;

namespace TestOrder.Presentation.Models;

public partial class Comment
{
    public long CommentId { get; set; }

    public Guid? TestId { get; set; }

    public string? Comment1 { get; set; }

    public DateOnly? CommentDate { get; set; }

    public virtual Booking? Test { get; set; }
}
