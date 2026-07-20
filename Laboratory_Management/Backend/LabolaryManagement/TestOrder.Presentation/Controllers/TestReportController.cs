using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestOrder.Application.Services;
using TestOrder.Infrastructure.Models;

namespace TestOrder.Presentation.Controllers
{
    [Route("api/test-reports")]
    [ApiController]
    [Tags("Test Reports")]
    public class TestReportController : ControllerBase
    {
        private readonly TestReportService _testReportService;
        public TestReportController(TestReportService testReportService)
        {
            _testReportService = testReportService;
        }

        [HttpPost("/api/bookings/{bookingId:guid}/reports")]
        public async Task<IActionResult> GenerateReport(Guid bookingId)
        {
            try
            {
                var reportData = await _testReportService.CreateNewReport(bookingId);
                return StatusCode(201, new { message = "Report generated and saved successfully." });
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while generating the report.", details = ex.Message });
            }
        }

        [HttpGet("/api/bookings/{bookingId:guid}/reports")]
        public async Task<IActionResult> GetReportByBookingId(Guid bookingId)
        {
            var report = await _testReportService.GetReportByBookingId(bookingId);
            if (report == null)
            {
                return NotFound(new { message = "No report found for the specified booking ID." });
            }
            return Ok(report);
        }

        [HttpGet("/api/bookings/{bookingId:guid}/reports/file")]
        public async Task<IActionResult> DownloadReport(Guid bookingId)
        {
            var report = await _testReportService.GetReportByBookingId(bookingId);
            if (report == null || report.ResultData == null)
            {
                return NotFound(new { message = "No report found for the specified booking ID." });
            }
            return File(report.ResultData, report.FileType, report.Filename);
        }
    }
}
