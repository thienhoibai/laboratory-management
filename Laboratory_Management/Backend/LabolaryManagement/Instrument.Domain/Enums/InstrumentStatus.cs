namespace Instrument.Domain.Enums;

/// <summary>
/// Trạng thái hoạt động của máy xét nghiệm
/// </summary>
public enum InstrumentStatus : int
{
    /// <summary>
    /// Máy đang hoạt động bình thường
    /// </summary>
    Online = 0,
    
    /// <summary>
    /// Máy đang offline
    /// </summary>
    Offline = 1,
    
    /// <summary>
    /// Máy bị lỗi
    /// </summary>
    Fault = 2,
    
    /// <summary>
    /// Máy đang bảo trì
    /// </summary>
    Maintenance = 3
}
