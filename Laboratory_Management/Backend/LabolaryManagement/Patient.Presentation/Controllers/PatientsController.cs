using System.Security.Claims;
using Common.Errors;
using Common.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Patient.Application.DTOs;
using Patient.Application.Services;

namespace Patient.Presentation.Controllers;

[Authorize]
[ApiController]
[Route("v1/patients")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _svc;
    public PatientsController(IPatientService svc) => _svc = svc;

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
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? name = null, [FromQuery] DateOnly? dob = null, [FromQuery] bool? isDeleted = null, [FromQuery] string? sortBy = null, [FromQuery] string? sort = null, CancellationToken ct = default)
    {
        var (items, total) = await _svc.ListAsync(page, pageSize, name, dob, isDeleted, sortBy, sort, ct);
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

    [HttpGet("{id:guid}/versions")]
    public async Task<IActionResult> Versions(Guid id, [FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? sort = null, CancellationToken ct = default)
    {
        var (items, total) = await _svc.GetVersionsAsync(id, page, pageSize, sort, ct);
        var meta = new Common.Responses.PageMeta { Page = page, PageSize = pageSize, TotalItems = total, TotalPages = (int)Math.Ceiling(total / (double)pageSize) };
        return Ok(ApiResponse.Success(items, meta, TraceId));
    }
}
