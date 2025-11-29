namespace Instrument.Domain.Enums;

/// <summary>
/// Trạng thái của một lần chạy xét nghiệm (run)
/// </summary>
public enum RunStatus : int
{
    /// <summary>
    /// Đang chạy
    /// </summary>
    Running = 0,
    
    /// <summary>
    /// Hoàn thành
    /// </summary>
    Completed = 1,
    
    /// <summary>
    /// Thất bại
    /// </summary>
    Failed = 2
}
