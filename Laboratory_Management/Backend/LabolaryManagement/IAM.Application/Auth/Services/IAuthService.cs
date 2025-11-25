using Common.Results;
using IAM.Application.Auth.DTOs.Requests;
using IAM.Application.Auth.DTOs.Responses;
using IAM.Application.Users.DTOs.Responses;

namespace IAM.Application.Auth.Services;

public interface IAuthService
{
    Task<OperationResult<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken ct = default);
    Task<OperationResult<RefreshResponse>> RefreshAsync(RefreshRequest request, CancellationToken ct = default);
    Task<OperationResult> LogoutAsync(LogoutRequest request, CancellationToken ct = default);
    Task<OperationResult<UserDetailDto>> RegisterAsync(RegisterRequest request, CancellationToken ct = default);
    Task<OperationResult> ChangePasswordAsync(Guid userId, ChangePasswordRequest request, CancellationToken ct = default);
    Task<OperationResult> ForgotPasswordAsync(ForgotPasswordRequest request, CancellationToken ct = default);
    Task<OperationResult> ResetPasswordAsync(ResetPasswordRequest request, CancellationToken ct = default);
    Task<OperationResult<LoginResponse>> LoginWithGoogleAsync(GoogleLoginRequest request, string? ip = null, string? userAgent = null, CancellationToken ct = default);
}
