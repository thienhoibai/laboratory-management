using System;
using System.Collections.Generic;

namespace IAM.Domain.Entities
{
    public class Role
    {
        public int RoleId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsDefault { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public ICollection<Permission> Permissions { get; set; } = new List<Permission>();
        public ICollection<User> Users { get; set; } = new List<User>();
    }
}
