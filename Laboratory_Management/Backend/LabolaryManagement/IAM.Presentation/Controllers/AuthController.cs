using IAM.Application.Auth;
using IAM.Application.Auth.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Common.Web.Filters;
using System.Security.Claims;

namespace IAM.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        public AuthController(IAuthService auth) => _auth = auth;

        private static Guid GetActorId(ClaimsPrincipal user)
        {
            var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
            return id != null && Guid.TryParse(id, out var g) ? g : Guid.Empty;
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
        {
            var result = await _auth.LoginAsync(request, ct);
            if (!result.Succeeded)
            {
                var code = result.Error ?? Common.Errors.ErrorCodes.InvalidCredentials;
                var detail = code switch
                {
                    var c when c == Common.Errors.ErrorCodes.InvalidCredentials => "Tên đăng nhập hoặc mật khẩu không đúng.",
                    var c when c == Common.Errors.ErrorCodes.AccountLocked => "Tài khoản đang bị khóa. Vui lòng thử lại sau.",
                    _ => null
                };
                throw new ApiException(code, detail);
            }
            Response.Headers.CacheControl = "no-store";
            Response.Headers.Pragma = "no-cache";
            return Ok(result.Data);
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
        {
            var result = await _auth.RegisterAsync(request, ct);
            if (!result.Succeeded)
            {
                var code = result.Error ?? Common.Errors.ErrorCodes.ValidationError;
                var detail = code == Common.Errors.ErrorCodes.DuplicateUsername ? "Tên đăng nhập đã được sử dụng." :
                             code == Common.Errors.ErrorCodes.DuplicateEmail ? "Email đã được sử dụng." : null;
                throw new ApiException(code, detail);
            }
            return CreatedAtAction(nameof(Register), new { id = result.Data!.UserId }, result.Data);
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request, CancellationToken ct)
        {
            var result = await _auth.RefreshAsync(request, ct);
            if (!result.Succeeded) throw new ApiException(Common.Errors.ErrorCodes.InvalidRefreshToken);
            Response.Headers.CacheControl = "no-store";
            Response.Headers.Pragma = "no-cache";
            return Ok(result.Data);
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] LogoutRequest request, CancellationToken ct)
        {
            var result = await _auth.LogoutAsync(request, ct);
            if (!result.Succeeded) throw new ApiException(Common.Errors.ErrorCodes.ValidationError);
            return NoContent();
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken ct)
        {
            var userId = GetActorId(User);
            if (userId == Guid.Empty) throw new ApiException(Common.Errors.ErrorCodes.Unauthorized);
            var res = await _auth.ChangePasswordAsync(userId, request, ct);
            if (!res.Succeeded) throw new ApiException(res.Error ?? Common.Errors.ErrorCodes.ValidationError);
            return Ok(new { changed = true });
        }

        [HttpPost("forgot-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken ct)
        {
            var res = await _auth.ForgotPasswordAsync(request, ct);
            if (!res.Succeeded) throw new ApiException(res.Error ?? Common.Errors.ErrorCodes.NotFound);
            return Ok(new { requested = true });
        }

        [HttpPost("reset-password")]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken ct)
        {
            var res = await _auth.ResetPasswordAsync(request, ct);
            if (!res.Succeeded) throw new ApiException(res.Error ?? Common.Errors.ErrorCodes.InvalidResetToken);
            return Ok(new { reset = true });
        }
    }
}
