using System;

namespace IAM.Domain.Entities
{
    public class UserSecurity
    {
        public Guid UserId { get; set; }
        public int FailedAccessCount { get; set; }
        public DateTime? LockoutEnd { get; set; }
        public User? User { get; set; }
    }
}
