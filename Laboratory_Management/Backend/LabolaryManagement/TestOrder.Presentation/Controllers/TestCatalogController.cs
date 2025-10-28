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
        public async Task<IActionResult> GetAllCatalogAsync()
        {
            var catalogs = await _service.GetAllCatalogAsync();
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

    }
}
