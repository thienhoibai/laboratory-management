using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using TestOrder.Application.DTOs;
using TestOrder.Application.Services;

namespace TestOrder.API.Controllers
{
    [ApiController]
    [Route("api/catalog-bundles")]
    [Tags("Catalog Bundles")]
    public class CatalogBundleController : ControllerBase
    {
        private readonly CatalogBundleService _service;

        public CatalogBundleController(CatalogBundleService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _service.GetAllAsync();
            return Ok(result);
        }

        [HttpGet("/api/test-bundles/{bundleId}/catalogs")]
        public async Task<IActionResult> GetCatalogsByBundle(int bundleId)
        {
            var result = await _service.GetCatalogsByBundleAsync(bundleId);
            if (result == null)
                return NotFound();
            return Ok(result);
        }

        [HttpPost]
        [Authorize(Policy = "perm:CatalogBundle.Create")]
        public async Task<IActionResult> AddCatalogToBundle([FromBody] CatalogBundleDTO dto)
        {
            try
            {
                await _service.AddCatalogToBundleAsync(dto);
                return StatusCode(201, new { message = "Catalog associated with bundle successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{bundleId}")]
        [Authorize(Policy = "perm:CatalogBundle.Delete")]
        public async Task<IActionResult> RemoveCatalogFromBundle(int bundleId, [FromQuery] List<int> catalogId)
        {
            await _service.RemoveCatalogFromBundleAsync(bundleId, catalogId);
            return NoContent();
        }
    }
}
