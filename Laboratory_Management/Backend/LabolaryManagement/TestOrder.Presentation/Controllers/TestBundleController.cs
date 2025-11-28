using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using TestOrder.Application.DTOs;
using TestOrder.Application.Services;
using TestOrder.Infrastructure.Models;

namespace TestOrder.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Gói xét nghiệm")]
    public class TestBundleController : ControllerBase
    {
        private readonly TestBundleService _service;

        public TestBundleController(TestBundleService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Policy = "perm:TestBundle.List")]
        public async Task<IActionResult> GetAllBundleAsync(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? search = null)
        {
            var result = await _service.GetAllBundleAsync(page, pageSize, search);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "perm:TestBundle.View")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var bundle = await _service.GetByIdAsync(id);
            if (bundle == null) return NotFound();
            return Ok(bundle);
        }

        [HttpPost]
        [Authorize(Policy = "perm:TestBundle.Create")]
        public async Task<IActionResult> Create(TestBundleDTO dto)
        {
            var entity = new TestBundle
            {
                BundleName = dto.BundleName,
                Description = dto.Description,
                Price = dto.Price,
                IsActive = dto.IsActive
            };

            await _service.AddBundleAsync(dto);
            return Ok(entity);
        }

        [HttpPut("{id}")]
        [Authorize(Policy = "perm:TestBundle.Update")]
        public async Task<IActionResult> UpdateBundleAsync(int id, [FromBody] UpdateBundleDTO model)
        {
            await _service.UpdateBundleAsync(id, model.BundleName, model.Description, model.Price);
            return NoContent();
        }

        [HttpDelete("{id}")]
        [Authorize(Policy = "perm:TestBundle.Delete")]
        public async Task<IActionResult> DeleteBundleAsync(int id)
        {
            await _service.DeleteBundleAsync(id);
            return NoContent();
        }
    }
}
