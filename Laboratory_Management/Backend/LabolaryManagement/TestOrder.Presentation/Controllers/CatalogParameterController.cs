using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using TestOrder.Application.DTOs;
using TestOrder.Application.Services;

namespace TestOrder.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CatalogParameterController : ControllerBase
    {
        private readonly CatalogParameterService _service;

        public CatalogParameterController(CatalogParameterService service)
        {
            _service = service;
        }

        //[HttpGet("{parameterId}")]
        //public async Task<IActionResult> GetCatalogsByParameter(int parameterId)
        //{
        //    var result = await _service.GetCatalogsByParameterAsync(parameterId);
        //    return Ok(result);
        //}

        [HttpPost]
        public async Task<IActionResult> AddCatalogToParameter([FromBody] CatalogParameterDTO dto)
        {
            await _service.AddCatalogToParameterAsync(dto);
            return Ok("Catalog added to bundle successfully");
        }

    }
}
