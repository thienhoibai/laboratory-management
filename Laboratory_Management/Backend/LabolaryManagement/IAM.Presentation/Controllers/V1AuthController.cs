using IAM.Application.Auth.Services;
using IAM.Application.Auth.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Common.Web.Filters;

namespace IAM.Presentation.Controllers
{
    [ApiController]
    [Route("v1/auth")]
    public class V1AuthController : ControllerBase
    {
        private readonly IAuthService _auth;
        public V1AuthController(IAuthService auth) => _auth = auth;

        [HttpPost("google")]
        [AllowAnonymous]
        public async Task<IActionResult> Google([FromBody] GoogleLoginRequest request, CancellationToken ct)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var ua = Request.Headers.UserAgent.ToString();
            var result = await _auth.LoginWithGoogleAsync(request, ip, ua, ct);
            if (!result.Succeeded) throw new ApiException(result.Error ?? Common.Errors.ErrorCodes.InvalidGoogleToken);
            return Ok(result.Data);
        }
    }
}
