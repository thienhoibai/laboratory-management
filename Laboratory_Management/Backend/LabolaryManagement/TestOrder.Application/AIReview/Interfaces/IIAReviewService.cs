using TestOrder.Application.AIReview.DTOs;

namespace TestOrder.Application.AIReview.Interfaces;

/// <summary>
/// Interface cho dịch vụ phân tích kết quả xét nghiệm sử dụng AI (Gemini)
/// </summary>
public interface IIAReviewService
{
    /// <summary>
    /// Phân tích danh sách kết quả xét nghiệm và trả về nhận xét từ AI
    /// </summary>
    /// <param name="testResults">Danh sách chỉ số xét nghiệm cần phân tích</param>
    /// <returns>Danh sách kết quả có kèm nhận xét từ AI</returns>
    Task<List<TestResultDetail>> AnalyzeTestResultAsync(List<TestInputItem> testResults);
}
