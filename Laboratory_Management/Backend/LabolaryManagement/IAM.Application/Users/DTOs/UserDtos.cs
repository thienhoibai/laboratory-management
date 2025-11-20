namespace IAM.Application.Users.DTOs
{
    // Admin creates account: admin nhập username, email, chọn role; password sẽ random và gửi qua email
    public record CreateUserRequest(string Username, string Email, int RoleId);
    public record UpdateUserRequest(string? Email, string? FullName); // removed Phone
    public record AssignRolesRequest(int[] RoleIds);

    public record UserSummaryDto(Guid UserId, string Username, string Email, string? FullName, bool IsActive, DateTime CreatedAt, DateTime? LastLoginAt, string[] Roles);
    public record UserDetailDto(Guid UserId, string Username, string Email, string? FullName, bool IsActive, DateTime? LastLoginAt, DateTime CreatedAt, DateTime UpdatedAt, string[] Roles);
}
