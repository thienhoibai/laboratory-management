using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TestOrder.Application.AIReview.DTOs;
using TestOrder.Application.AIReview.Interfaces;
using TestOrder.Infrastructure.Data;
using TestOrder.Infrastructure.Models;

namespace TestOrder.Presentation.Controllers;

[Route("api/ai-reviews")]
[ApiController]
[Tags("AI Reviews")]
public class AiReviewController : ControllerBase
{
    private readonly IIAReviewService _reviewService;
    private readonly TestOrderDBContext _db;

    public AiReviewController(IIAReviewService reviewService, TestOrderDBContext db)
    {
        _reviewService = reviewService;
        _db = db;
    }

    /// <summary>
    /// GET /api/ai-reviews/health
    /// Health check endpoint để kiểm tra controller hoạt động
    /// </summary>
    [HttpGet("health")]
    public IActionResult HealthCheck()
    {
        return Ok(new
        {
            status = "AI Review Controller is working!",
            timestamp = DateTime.Now,
            service = "TestOrder.Presentation",
            dbConnected = _db != null
        });
    }

    /// <summary>
    /// POST /api/bookings/{bookingId}/ai-reviews
    /// Tạo/Cập nhật nhận xét AI cho TẤT CẢ chỉ số trong một booking
    /// </summary>
    /// <param name="bookingId">ID của booking cần review</param>
    /// <returns>Danh sách kết quả có nhận xét từ AI</returns>
    [HttpPost("/api/bookings/{bookingId:guid}/ai-reviews")]
    public async Task<IActionResult> ReviewBooking(Guid bookingId)
    {
        // 1. Kiểm tra booking có tồn tại không
        var booking = await _db.Bookings.FindAsync(bookingId);
        if (booking == null)
            return NotFound(new { message = "Không tìm thấy booking." });

        // 2. Lấy danh sách TestBookingNo của booking này
        var testBookingNos = await _db.BookingTests
            .Where(bt => bt.BookingId == bookingId)
            .Select(bt => bt.TestBookingNo)
            .ToListAsync();

        if (!testBookingNos.Any())
            return NotFound(new { message = "Booking này chưa có test nào." });

        // 3. Lấy TẤT CẢ TestResult của booking này (kèm Parameter)
        var testResults = await _db.TestResults
            .Include(r => r.Parameter)
            .Where(r => r.TestBookingNo.HasValue && testBookingNos.Contains(r.TestBookingNo.Value))
            .ToListAsync();

        if (!testResults.Any())
            return NotFound(new { message = "Chưa có kết quả xét nghiệm." });

        // 4. Kiểm tra đã có Comment chưa (nếu có thì trả về luôn, không gọi lại AI)
        var hasComments = testResults.All(r => !string.IsNullOrEmpty(r.Comment));

        if (hasComments)
        {
            return Ok(new
            {
                message = "Đã có nhận xét từ AI (đã được tạo trước đó).",
                cached = true,
                results = testResults.Select(MapToDto).ToList()
            });
        }

        // 5. Chuẩn bị dữ liệu gửi cho AI
        var aiInputs = testResults.Select(r => new TestInputItem
        {
            Parameter = r.Parameter?.ParameterName ?? "N/A",
            Value = r.ResultValue ?? "N/A",
            Range = r.Parameter?.ReferenceRange ?? "N/A",
            MinRange = r.Parameter?.MinRange,
            MaxRange = r.Parameter?.MaxRange,
            Unit = r.Parameter?.Unit ?? "",
            Status = DetermineStatus(r.IsNormal, r.ResultValue, r.Parameter)
        }).ToList();

        try
        {
            // 6. GỌI AI REVIEW (một lần cho tất cả chỉ số)
            var aiReviews = await _reviewService.AnalyzeTestResultAsync(aiInputs);

            // 7. CẬP NHẬT Comment vào từng TestResult trong DB
            foreach (var review in aiReviews)
            {
                var testResult = testResults.FirstOrDefault(r =>
                    r.Parameter?.ParameterName == review.Parameter);

                if (testResult != null)
                {
                    testResult.Comment = review.Comment;
                    _db.Update(testResult);
                }
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                message = "Đã tạo nhận xét AI thành công.",
                cached = false,
                results = aiReviews
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                message = "Lỗi khi gọi AI",
                error = ex.Message
            });
        }
    }

    [HttpGet("/api/bookings/{bookingId:guid}/ai-reviews")]
    public async Task<IActionResult> GetReviewedResults(Guid bookingId)
    {
        var testBookingNos = await _db.BookingTests
            .Where(bt => bt.BookingId == bookingId)
            .Select(bt => bt.TestBookingNo)
            .ToListAsync();

        if (!testBookingNos.Any())
            return NotFound(new { message = "Booking này chưa có test nào." });

        var testResults = await _db.TestResults
            .Include(r => r.Parameter)
            .Where(r => r.TestBookingNo.HasValue && testBookingNos.Contains(r.TestBookingNo.Value))
            .ToListAsync();

        if (!testResults.Any())
            return NotFound(new { message = "Chưa có kết quả xét nghiệm." });

        var hasComments = testResults.Any(r => !string.IsNullOrEmpty(r.Comment));

        return Ok(new
        {
            hasAiReview = hasComments,
            totalResults = testResults.Count,
            results = testResults.Select(MapToDto).ToList()
        });
    }

    private static string DetermineStatus(bool? isNormal, string? resultValue, TestParameter? parameter)
    {
        // 1. Không có parameter hoặc result value
        if (parameter == null || string.IsNullOrEmpty(resultValue))
            return "Unknown";

        // 2. Parse giá trị kết quả
        if (!double.TryParse(resultValue, out var value))
            return "Unknown";

        // 3. Không có MinRange và MaxRange → chỉ dựa vào IsNormal
        if (!parameter.MinRange.HasValue || !parameter.MaxRange.HasValue)
        {
            return isNormal switch
            {
                true => "Normal",
                false => "Abnormal",
                null => "Unknown"
            };
        }

        // 4. SO SÁNH GIÁ TRỊ VỚI KHOẢNG THAM CHIẾU
        if (value < parameter.MinRange.Value)
            return "Low";

        if (value > parameter.MaxRange.Value)
            return "High";

        return "Normal";
    }

    private static object MapToDto(TestResult r)
    {
        return new
        {
            resultId = r.ResultId,
            testBookingNo = r.TestBookingNo,
            parameterId = r.ParameterId,
            parameter = r.Parameter?.ParameterName ?? "N/A",
            value = r.ResultValue ?? "N/A",
            range = r.Parameter?.ReferenceRange ?? "N/A",
            minRange = r.Parameter?.MinRange,
            maxRange = r.Parameter?.MaxRange,
            unit = r.Parameter?.Unit ?? "",
            isNormal = r.IsNormal,
            status = DetermineStatus(r.IsNormal, r.ResultValue, r.Parameter),
            comment = r.Comment
        };
    }
}
