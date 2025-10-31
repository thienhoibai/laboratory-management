using Common.Errors;
using Common.Responses;
using Common.Web.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Patient.Application.DTOs;
using Patient.Application.Services;
using System.Diagnostics;
using System.Security.Claims;


namespace Patient.Presentation.Controllers;

[ApiController]
[Route("v1/patients")]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _service;
    public PatientsController(IPatientService service)
    {
        _service = service;
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
        return id != null && Guid.TryParse(id, out var g) ? g : Guid.Empty;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CreatePatientRequest request, CancellationToken ct)
    {
        var userId = GetUserId(User);
        if (userId == Guid.Empty) return Unauthorized();
        var result = await _service.CreateAsync(request, userId, null, ct);
        if (!result.Succeeded) return BadRequest(new { error = result.Error?.ToString() });
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.PatientId }, result.Data);
    }


    [HttpGet("me")]
    public async Task<IActionResult> GetMyPatient(CancellationToken ct)
    {
        var userId = GetUserId(User);

        if (userId == Guid.Empty)
            return Unauthorized(); ;

        var result = await _service.GetByUserIdAsync(userId, ct);
        if (result == null)
            return NotFound();
             return Ok(result);
    }

    [HttpGet("mine")]
    [Authorize]
    public async Task<IActionResult> Mine(int page = 1, int pageSize = 50, string? name = null, CancellationToken ct = default)
    {
        var userId = GetUserId(User);
        if (userId == Guid.Empty) return Unauthorized();
        var (items, total) = await _service.ListByOwnerAsync(userId, page, pageSize, name, null, null, null, ct);
        return Ok(new { total, items });
    }

    [HttpGet("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
                                                                                                                                                                                                 
    {
        var userId = GetUserId(User);
        var res = await _service.GetAsync(id, ct);
        if (!res.Succeeded || res.Data == null) return NotFound();
        var isOwner = await _service.IsOwnerAsync(id, userId, ct);
        if (!isOwner && !User.IsInRole("Admin")) return Forbid();
        return Ok(res.Data);
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePatientRequest request, CancellationToken ct)
    {
        var userId = GetUserId(User);
        if (userId == Guid.Empty) return Unauthorized();
        var res = await _service.UpdateAsync(id, request, userId, null, ct);
        if (!res.Succeeded)
        {
            if (res.Error == Common.Errors.ErrorCodes.Forbidden) return Forbid();
            if (res.Error == Common.Errors.ErrorCodes.NotFound) return NotFound();
            return BadRequest(new { error = res.Error?.ToString() });
        }
        return Ok(res.Data);
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var userId = GetUserId(User);
        if (userId == Guid.Empty) return Unauthorized();
        var res = await _service.DeleteAsync(id, userId, null, null, ct);
        if (!res.Succeeded)
        {
            if (res.Error == Common.Errors.ErrorCodes.Forbidden) return Forbid();
            if (res.Error == Common.Errors.ErrorCodes.NotFound) return NotFound();
            return BadRequest(new { error = res.Error?.ToString() });
        }
        return NoContent();
    }
}
