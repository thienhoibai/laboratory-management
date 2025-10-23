using Microsoft.AspNetCore.Mvc;
using Patient.Application;
using System.Threading.Tasks;

namespace Patient.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PatientController : ControllerBase
    {
        private readonly PatientService _service;

        public PatientController(PatientService service)
        {
            _service = service;
        }

        
        [HttpGet]
        public async Task<IActionResult> GetAllPatients()
        {
            var patients = await _service.GetAllPatientsAsync();
            return Ok(patients);
        }

        
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPatientById(int id)
        {
            var patient = await _service.GetPatientByIdAsync(id);
            if (patient == null)
                return NotFound(new { message = $"Patient with ID {id} not found." });

            return Ok(patient);
        }
    }
}
