using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TestOrder.Application.Services;
using TestOrder.Infrastructure.Models;

namespace TestOrder.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Tags("Báo cáo Kết quả")]
    public class TestReportController : ControllerBase
    {
        private readonly TestReportService _testReportService;
        public TestReportController(TestReportService testReportService)
        {
            _testReportService = testReportService;
        }
        [HttpGet("GenerateReport/{bookingId}")]
        public async Task<IActionResult> GenerateReport(Guid bookingId)
        {
            try
            {
                var reportData = await _testReportService.CreateNewReport(bookingId);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while generating the report.", details = ex.Message });
            }
            return Ok(new { message = "Report generated and saved successfully." });
        }

        [HttpGet]
        public async Task<IActionResult> GetReportByBookingId(Guid bookingId)
        {
            var report = await _testReportService.GetReportByBookingId(bookingId);
            if (report == null)
            {
                return NotFound(new { message = "No report found for the specified booking ID." });
            }
            return Ok(report);
        }

        [HttpGet]
        [Route("DownloadReport/{bookingId}")]

        public async Task<IActionResult> DownloadReport(Guid bookingId)
        {
            var report = await _testReportService.GetReportByBookingId(bookingId);
            if (report == null || report.ResultData == null)
            {
                return NotFound(new { message = "No report found for the specified booking ID." });
            }
            return File(report.ResultData, report.FileType , report.Filename);
        }
    }
}
