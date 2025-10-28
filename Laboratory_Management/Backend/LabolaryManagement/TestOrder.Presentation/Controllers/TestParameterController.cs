using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> AddParameterAsync([FromBody] TestParameterDTO parameter)
        {
           var entity = new TestParameter
            {
                ParameterName = parameter.ParameterName,
                Unit = parameter.Unit,
                ReferenceRange = parameter.ReferenceRange,
           };
            await _service.AddParameterAsync(parameter);
            return Ok(entity);
        }
    }
}
