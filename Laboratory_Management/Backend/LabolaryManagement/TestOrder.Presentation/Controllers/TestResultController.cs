using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using TestOrder.Application.Services;

namespace TestOrder.Presentation.Controllers
{
    [Route("api/test-results")]
    [ApiController]
    [Tags("Test Results")]
    public class TestResultController : ControllerBase
    {
        private readonly TestResultService _testResultService;
        public TestResultController(TestResultService testResultService)
        {
            _testResultService = testResultService;
        }

        [HttpGet("/api/bookings/{bookingId:guid}/results")]
        public async Task<IActionResult> GetTestResultByBookingId(Guid bookingId)
        {
            var result = await _testResultService.GetTestResultByBookingId(bookingId);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
    }
}
