using System;
using System.Collections.Generic;

namespace IAM.Domain.Entities
{
    public class Permission
    {
        public int PermissionId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<Role> Roles { get; set; } = new List<Role>();
    }
}
