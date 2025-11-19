using System;
using System.Collections.Generic;

namespace TestOrder.Infrastructure.Models;

public partial class AuditLog
{
    public Guid AuditLogId { get; set; }

    public string? Action { get; set; }

    public string? Description { get; set; }

    public Guid? UserId { get; set; }
}
