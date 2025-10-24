using System.Security.Claims;
using Common.Errors;
using Common.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Patient.Application.DTOs;
using Patient.Application.Services;
using Microsoft.EntityFrameworkCore;
using Patient.Infrastructure;

namespace Patient.Presentation.Controllers;

[Authorize]
[ApiController]
[Route("v1/patients")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _svc;
    private readonly PatientDbContext _db;
    public PatientsController(IPatientService svc, PatientDbContext db) { _svc = svc; _db = db; }

    private string TraceId => HttpContext.TraceIdentifier;
    private Guid GetUserId() => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub")!);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreatePatientRequest req, CancellationToken ct)
    {
        var actor = GetUserId();
        var result = await _svc.CreateAsync(req, actor, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        if (!result.Succeeded) throw new Common.Web.Filters.ApiException(result.Error ?? ErrorCodes.ValidationError);
        return Ok(ApiResponse.Success(result.Data, null, TraceId));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePatientRequest req, CancellationToken ct)
    {
        var actor = GetUserId();
        if (!await _svc.IsOwnerAsync(id, actor, ct)) return Forbid();
        var result = await _svc.UpdateAsync(id, req, actor, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        if (!result.Succeeded) throw new Common.Web.Filters.ApiException(result.Error ?? ErrorCodes.ValidationError);
        return Ok(ApiResponse.Success(result.Data, null, TraceId));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] string? reason, CancellationToken ct)
    {
        var actor = GetUserId();
        if (!await _svc.IsOwnerAsync(id, actor, ct)) return Forbid();
        var result = await _svc.DeleteAsync(id, actor, reason, HttpContext.Connection.RemoteIpAddress?.ToString(), ct);
        if (!result.Succeeded) throw new Common.Web.Filters.ApiException(result.Error ?? ErrorCodes.NotFound);
        return Ok(ApiResponse.Success(new { deleted = true, patientId = id }, null, TraceId));
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? name = null,
        [FromQuery] DateOnly? dob = null,
        [FromQuery] bool? isDeleted = null,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sort = null,
        [FromQuery] string? owner = null,
        [FromQuery] string? idLast4 = null,
        [FromQuery] string? phoneLast4 = null,
        CancellationToken ct = default)
    {
        // Optional filter owner=me for backward compatibility
        if (string.Equals(owner, "me", StringComparison.OrdinalIgnoreCase))
        {
            var userId = GetUserId();
            var q = _db.PatientOwners.AsNoTracking().Where(x => x.UserId == userId).Select(x => x.Patient).AsQueryable();
            if (!string.IsNullOrWhiteSpace(name))
            {
                var norm = name.Trim().ToUpperInvariant();
                q = q.Where(p => p.FullNameNorm != null && p.FullNameNorm.Contains(norm));
            }
            if (dob.HasValue) q = q.Where(p => p.DateOfBirth == dob);
            if (isDeleted.HasValue) q = q.Where(p => p.IsDeleted == isDeleted.Value);
            if (!string.IsNullOrWhiteSpace(phoneLast4)) q = q.Where(p => p.PhoneLast4 == phoneLast4);
            if (!string.IsNullOrWhiteSpace(idLast4)) q = q.Where(p => p.IdLast4 == idLast4);

            q = sortBy?.ToLowerInvariant() switch
            {
                "name" => (sort?.ToLowerInvariant() == "desc" ? q.OrderByDescending(x => x.FullNameNorm) : q.OrderBy(x => x.FullNameNorm)),
                "createdat" => (sort?.ToLowerInvariant() == "asc" ? q.OrderBy(x => x.CreatedAt) : q.OrderByDescending(x => x.CreatedAt)),
                "updatedat" => (sort?.ToLowerInvariant() == "asc" ? q.OrderBy(x => x.UpdatedAt) : q.OrderByDescending(x => x.UpdatedAt)),
                _ => q.OrderByDescending(x => x.CreatedAt)
            };

            var totalMine = await q.LongCountAsync(ct);
            var itemsMine = await q.Skip((page - 1) * pageSize).Take(pageSize)
                .Select(e => new PatientSummaryDto(e.PatientId, e.FullNameNorm, e.DateOfBirth, e.Gender, e.PhoneLast4, e.IsDeleted, e.CreatedAt, e.UpdatedAt))
                .ToListAsync(ct);
            var metaMine = new Common.Responses.PageMeta { Page = page, PageSize = pageSize, TotalItems = totalMine, TotalPages = (int)Math.Ceiling(totalMine / (double)pageSize) };
            return Ok(ApiResponse.Success(itemsMine, metaMine, TraceId));
        }

        var (items, total) = await _svc.ListAsync(page, pageSize, name, dob, isDeleted, sortBy, sort, idLast4, phoneLast4, ct);
        var meta = new Common.Responses.PageMeta { Page = page, PageSize = pageSize, TotalItems = total, TotalPages = (int)Math.Ceiling(total / (double)pageSize) };
        return Ok(ApiResponse.Success(items, meta, TraceId));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct)
    {
        var result = await _svc.GetAsync(id, ct);
        if (!result.Succeeded) throw new Common.Web.Filters.ApiException(result.Error ?? ErrorCodes.NotFound);
        return Ok(ApiResponse.Success(result.Data, null, TraceId));
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine(
        [FromQuery] int page = 1,
        [FromQuery] int size = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sort = null,
        CancellationToken ct = default)
    {
        var userId = GetUserId();
        var query = _db.PatientOwners.Where(x => x.UserId == userId).Select(x => x.Patient);

        query = sortBy?.ToLowerInvariant() switch
        {
            "name" => (sort?.ToLowerInvariant() == "desc" ? query.OrderByDescending(x => x.FullNameNorm) : query.OrderBy(x => x.FullNameNorm)),
            "createdat" => (sort?.ToLowerInvariant() == "asc" ? query.OrderBy(x => x.CreatedAt) : query.OrderByDescending(x => x.CreatedAt)),
            "updatedat" => (sort?.ToLowerInvariant() == "asc" ? query.OrderBy(x => x.UpdatedAt) : query.OrderByDescending(x => x.UpdatedAt)),
            _ => query.OrderByDescending(x => x.CreatedAt)
        };

        var total = await query.LongCountAsync(ct);
        var items = await query
            .Skip((page - 1) * size)
            .Take(size)
            .Select(e => new PatientSummaryDto(e.PatientId, e.FullNameNorm, e.DateOfBirth, e.Gender, e.PhoneLast4, e.IsDeleted, e.CreatedAt, e.UpdatedAt))
            .ToListAsync(ct);
        var meta = new Common.Responses.PageMeta { Page = page, PageSize = size, TotalItems = total, TotalPages = (int)Math.Ceiling(total / (double)size) };
        return Ok(ApiResponse.Success(items, meta, TraceId));
    }

    [HttpGet("{id:guid}/versions")]
    public async Task<IActionResult> Versions(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? sort = null, CancellationToken ct = default)
    {
        var (items, total) = await _svc.GetVersionsAsync(id, page, pageSize, sort, ct);
        var meta = new Common.Responses.PageMeta { Page = page, PageSize = pageSize, TotalItems = total, TotalPages = (int)Math.Ceiling(total / (double)pageSize) };
        return Ok(ApiResponse.Success(items, meta, TraceId));
    }
}
