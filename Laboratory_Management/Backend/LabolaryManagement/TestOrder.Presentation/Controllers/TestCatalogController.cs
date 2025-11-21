using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TestOrder.Application.DTOs;
using TestOrder.Application.Services;
using TestOrder.Infrastructure.Models;


namespace TestOrder.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Danh mục xét nghiệm")]
    public class TestCatalogController : ControllerBase
    {
        private readonly TestCatalogService _service;

        public TestCatalogController(TestCatalogService service)
        {
            _service = service;
        }

        [HttpGet]

        public async Task<IActionResult> GetAllCatalogAsync(
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] string? search = null)

        {
            var catalogs = await _service.GetAllCatalogAsync(page, pageSize, search);
            return Ok(catalogs);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var catalog = await _service.GetByIdAsync(id);
            if (catalog == null) return NotFound();
            return Ok(catalog);
        }

        [HttpPost]
        public async Task<IActionResult> AddCatalogAsync([FromBody] TestCatalogDTO catalog)
        {
            var entity = new TestCatalog
            {
                TestName = catalog.TestName,
                Description = catalog.Description,
                Price = catalog.Price,
            };
            await _service.AddCatalogAsync(catalog);
            return Ok(entity);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCatalogAsync(int id, [FromBody] UpdateCatalogDTO model)
        {
            await _service.UpdateCatalogAsync(id, model.Description, model.Price);
            return NoContent();
        }
        [HttpPut]
        [Route("{id}/parameters")]
        public async Task<IActionResult> AddParameterAsync( int id, [FromBody] List<int>ParameterIds)
        { 
            var AddCatalog = await _service.AddParameterAsync(id, ParameterIds);
            return Ok(AddCatalog);
        }

        [HttpDelete]
        [Route("{id}/paramters")]
        public async Task<IActionResult> RemoveParameterAsync(int id, [FromBody] List<int> parametersIds)
        {
            await _service.RemoveParameterAsync(id, parametersIds);
            return Ok();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCatalogAsync(int id)
        {
            await _service.DeleteCatalogAsync(id);
            return Ok();
        }

    }
}
