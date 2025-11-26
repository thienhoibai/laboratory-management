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
    [Tags("Thông số xét nghiệm")]
    public class TestParameterController : ControllerBase
    {
        private readonly TestParameterService _service;

        public TestParameterController(TestParameterService service)
        {
            _service = service;
        }

        [HttpGet]
        [Authorize(Policy = "perm:TestParameter.List")]
        public async Task<IActionResult> GetAllParametersAsync(
            [FromQuery] int page,
            [FromQuery] int pageSize,
            [FromQuery] string? search = null)
        {
            var result = await _service.GetAllParameterAsync(page, pageSize, search);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [Authorize(Policy = "perm:TestParameter.View")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var parameter = await _service.GetByIdAsync(id);
            if (parameter == null) return NotFound();
            return Ok(parameter);
        }

        [HttpPost]
        [Authorize(Policy = "perm:TestParameter.Create")]
        public async Task<IActionResult> AddParameterAsync([FromBody] TestParameterDTO parameter)
        {
            var entity = new TestParameter
            {
                ParameterName = parameter.ParameterName,
                Unit = parameter.Unit,
                ReferenceRange = parameter.ReferenceRange,
                MinRange = parameter.MinRange,
                MaxRange = parameter.MaxRange
            };
            await _service.AddParameterAsync(parameter);
            return Ok(entity);
        }
    }
}
