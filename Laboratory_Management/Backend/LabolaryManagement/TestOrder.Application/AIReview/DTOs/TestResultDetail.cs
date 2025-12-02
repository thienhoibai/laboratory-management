namespace TestOrder.Application.AIReview.DTOs;

/// <summary>
/// DTO đầu ra cho kết quả xét nghiệm có nhận xét từ AI
/// </summary>
public class TestResultDetail
{
    /// <summary>
    /// Tên chỉ số (VD: WBC, HGB, Glucose)
    /// </summary>
    public string Parameter { get; set; } = string.Empty;

    /// <summary>
    /// Giá trị kết quả (VD: "3.2", "14.5")
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Khoảng tham chiếu (VD: "4.0-10.0", "12.0-16.0")
    /// </summary>
    public string Range { get; set; } = string.Empty;

    /// <summary>
    /// Đơn vị (VD: "10^9/L", "g/dL", "mg/dL")
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Trạng thái: "Normal", "Low", "High", "Abnormal", "Unknown"
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Nhận xét từ AI (được tạo bởi Gemini)
    /// </summary>
    public string Comment { get; set; } = string.Empty;
}
