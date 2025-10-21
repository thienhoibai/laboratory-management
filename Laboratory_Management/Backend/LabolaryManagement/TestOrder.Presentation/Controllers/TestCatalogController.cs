using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TestOrder.Application.DTOs;
using TestOrder.Application.Services;
using TestOrder.Infrastructure.Models;


namespace TestOrder.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
        public async Task<IActionResult> AddCatalogAsync([FromBody] TestCatalog catalog)
        {
            await _service.AddCatalogAsync(catalog);
            return CreatedAtAction(nameof(GetByIdAsync), new { id = catalog.CatalogId }, catalog);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCatalogAsync(int id, [FromBody] UpdateCatalogDTO model)
        {
            await _service.UpdateCatalogAsync(id, model.Description, model.Price);
            return NoContent();
        }

    }
}
