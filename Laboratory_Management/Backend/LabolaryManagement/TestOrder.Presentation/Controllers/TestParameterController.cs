using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TestOrder.Application.Services;
using TestOrder.Infrastructure.Models;

namespace TestOrder.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestParameterController : ControllerBase
    {
        private readonly TestParameterService _service;

        public TestParameterController(TestParameterService service)
        {
            _service = service;
        }
        [HttpGet]
        public async Task<IActionResult> GetAllParametersAsync()
        {
            var parameters = await _service.GetAllParametersAsync();
            return Ok(parameters);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetByIdAsync(int id)
        {
            var parameter = await _service.GetByIdAsync(id);
            if (parameter == null) return NotFound();
            return Ok(parameter);
        }
        [HttpPost]
        public async Task<IActionResult> AddParameterAsync([FromBody] TestParameter parameter)
        {
            await _service.AddParameterAsync(parameter);
            return CreatedAtAction(nameof(GetByIdAsync), new { id = parameter.ParameterId }, parameter);
        }
    }
}
