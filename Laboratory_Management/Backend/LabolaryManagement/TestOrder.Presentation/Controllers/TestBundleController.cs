using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TestOrder.Application.DTOs;
using TestOrder.Application.Services;
using TestOrder.Infrastructure.Models;

namespace TestOrder.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestBundleController : ControllerBase
    {
        private readonly TestBundleService _service;

        public TestBundleController(TestBundleService service)
        {
            _service = service;
        }

        // GET: api/TestBundle
        [HttpGet]
        public async Task<IActionResult> GetAllBundlesAsync()
        {
            var bundles = await _service.GetAllBundlesAsync();
            return Ok(bundles);
        }

        // GET: api/TestBundle/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var bundle = await _service.GetByIdAsync(id);
            if (bundle == null) return NotFound();
            return Ok(bundle);
        }

        // POST: api/TestBundle
        [HttpPost]
        public async Task<IActionResult> AddBundleAsync([FromBody] TestBundle bundle)
        {
            await _service.AddBundleAsync(bundle);
            return CreatedAtAction(nameof(GetByIdAsync), new { id = bundle.BundleId }, bundle);
        }

        // PUT: api/TestBundle/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateBundleAsync(int id, [FromBody] UpdateBundleDTO model)
        {
            await _service.UpdateBundleAsync(id, model.BundleName, model.Description, model.Price);
            return NoContent();
        }
    }
}
