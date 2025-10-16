using System;

namespace IAM.Domain.Entities
{
    public class User
    {
        public Guid UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginAt { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string AuthProvider { get; set; } = "Password"; // Password or Google
        public DateTime? EmailVerifiedAt { get; set; }

        public ICollection<Role> Roles { get; set; } = new List<Role>();
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}
