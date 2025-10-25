using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TestOrder.Application.DTOs;
using TestOrder.Application.Services;

namespace TestOrder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogBundleController : ControllerBase
    {
        private readonly CatalogBundleService _service;

        public CatalogBundleController(CatalogBundleService service)
        {
            _service = service;
        }

        [HttpGet("{bundleId}")]
        public async Task<IActionResult> GetCatalogsByBundle(int bundleId)
        {
            var result = await _service.GetCatalogsByBundleAsync(bundleId);
            return Ok(result);
        }

        [HttpPost]
        public async Task<IActionResult> AddCatalogToBundle([FromBody] CatalogBundleDTO dto)
        {
            await _service.AddCatalogToBundleAsync(dto);
            return Ok("Catalog added to bundle successfully");
        }

        [HttpDelete("{bundleId}/{catalogId}")]
        public async Task<IActionResult> RemoveCatalogFromBundle(int bundleId, int catalogId)
        {
            await _service.RemoveCatalogFromBundleAsync(bundleId, catalogId);
            return Ok("Catalog removed from bundle successfully");
        }
    }
}
