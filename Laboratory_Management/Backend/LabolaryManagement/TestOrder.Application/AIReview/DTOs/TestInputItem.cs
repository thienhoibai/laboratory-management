namespace TestOrder.Application.AIReview.DTOs;

/// <summary>
/// DTO đầu vào cho một chỉ số xét nghiệm cần phân tích bởi AI
/// </summary>
public class TestInputItem
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
    /// Khoảng tham chiếu dạng string (VD: "4.0-10.0", "12.0-16.0")
    /// </summary>
    public string Range { get; set; } = string.Empty;

    /// <summary>
    /// Giá trị MIN của khoảng tham chiếu (dùng để so sánh chính xác)
    /// </summary>
    public double? MinRange { get; set; }

    /// <summary>
    /// Giá trị MAX của khoảng tham chiếu (dùng để so sánh chính xác)
    /// </summary>
    public double? MaxRange { get; set; }

    /// <summary>
    /// Đơn vị (VD: "10^9/L", "g/dL", "mg/dL")
    /// </summary>
    public string Unit { get; set; } = string.Empty;

    /// <summary>
    /// Trạng thái: "Normal", "Low", "High", "Abnormal", "Unknown"
    /// </summary>
    public string Status { get; set; } = string.Empty;
}
