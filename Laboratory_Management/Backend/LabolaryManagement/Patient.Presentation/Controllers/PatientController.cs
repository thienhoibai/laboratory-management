using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Patient.Application.Services;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Patient.Presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PatientController : ControllerBase
    {
        private readonly IPatientService _service;

        public PatientController(IPatientService service)
        {
            _service = service;
        }

        private static Guid GetUserId(ClaimsPrincipal user)
        {
            var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
            return id != null && Guid.TryParse(id, out var g) ? g : Guid.Empty;
        }

        // Admin-only: list tất cả bệnh nhân
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllPatients(int page = 1, int pageSize = 20)
        {
            var (items, total) = await _service.ListAsync(page, pageSize, null, null, null, null, null, null, default);
            return Ok(new { total, items });
        }

        // Get chi tiết: chỉ chủ sở hữu hoặc Admin
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetPatientById(Guid id)
        {
            var userId = GetUserId(User);
            if (userId == Guid.Empty) return Unauthorized();
            var res = await _service.GetAsync(id, default);
            if (!res.Succeeded || res.Data == null)
                return NotFound(new { message = $"Patient with ID {id} not found." });

            var isOwner = await _service.IsOwnerAsync(id, userId, default);
            if (!isOwner && !User.IsInRole("Admin")) return Forbid();

            return Ok(res.Data);
        }
    }
}
