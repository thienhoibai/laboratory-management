using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using TestOrder.Application.DTOs;
using TestOrder.Application.Services;
using TestOrder.Infrastructure.Models;

namespace TestOrder.Presentation.Controllers
{
    [Route("api/test-catalogs")]
    [ApiController]
    [Tags("Test Catalogs")]
    public class TestCatalogController : ControllerBase
    {
        private readonly TestCatalogService _service;

        public TestCatalogController(TestCatalogService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Policy = "perm:TestCatalog.List")]
        public async Task<IActionResult> GetAllCatalogAsync(
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] string? search = null)
        {
            var catalogs = await _service.GetAllCatalogAsync(page, pageSize, search);
            return Ok(catalogs);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "perm:TestCatalog.View")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var catalog = await _service.GetByIdAsync(id);
            if (catalog == null) return NotFound();
            return Ok(catalog);
        }

        [HttpPost]
        [Authorize(Policy = "perm:TestCatalog.Create")]
        public async Task<IActionResult> AddCatalogAsync([FromBody] TestCatalogDTO catalog)
        {
            var entity = new TestCatalog
            {
                TestName = catalog.TestName,
                Description = catalog.Description,
                Price = catalog.Price,
            };
            await _service.AddCatalogAsync(catalog);
            return StatusCode(201, entity);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "perm:TestCatalog.Update")]
        public async Task<IActionResult> UpdateCatalogAsync(int id, [FromBody] UpdateCatalogDTO model)
        {
            await _service.UpdateCatalogAsync(id, model.Description, model.Price);
            return NoContent();
        }

        [HttpPost("{id}/parameters")]
        [Authorize(Policy = "perm:TestCatalog.UpdateParameter")]
        public async Task<IActionResult> AddParameterAsync(int id, [FromBody] List<int> parameterIds)
        { 
            var addCatalog = await _service.AddParameterAsync(id, parameterIds);
            return Ok(addCatalog);
        }

        [HttpDelete("{id}/parameters")]
        [Authorize(Policy = "perm:TestCatalog.DeleteParameter")]
        public async Task<IActionResult> RemoveParameterAsync(int id, [FromBody] List<int> parameterIds)
        {
            await _service.RemoveParameterAsync(id, parameterIds);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "perm:TestCatalog.Delete")]
        public async Task<IActionResult> DeleteCatalogAsync(int id)
        {
            await _service.DeleteCatalogAsync(id);
            return NoContent();
        }
    }
}
