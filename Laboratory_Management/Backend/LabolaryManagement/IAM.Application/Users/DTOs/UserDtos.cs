namespace IAM.Application.Users.DTOs
{
    // Admin creates account with minimal fields (role by id)
    public record CreateUserRequest(string Username, string Password, int RoleId);
    public record UpdateUserRequest(string? Email, string? FullName, string? Phone);
    public record AssignRolesRequest(int[] RoleIds);
    public record VerifyUserRequest(bool Approved, string? Note);
    public record LinkPatientRequest(string PatientId);

    public record UserSummaryDto(Guid UserId, string Username, string Email, string? FullName, bool IsActive, DateTime CreatedAt, DateTime? LastLoginAt, string[] Roles);
    public record UserDetailDto(Guid UserId, string Username, string Email, string? FullName, bool IsActive, DateTime? LastLoginAt, DateTime CreatedAt, DateTime UpdatedAt, string[] Roles);
}
