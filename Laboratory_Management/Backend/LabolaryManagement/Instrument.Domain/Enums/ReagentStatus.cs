namespace Instrument.Domain.Enums;

/// <summary>
/// Trạng thái hóa chất/reagent của máy xét nghiệm
/// </summary>
public enum ReagentStatus : byte
{
    /// <summary>
    /// Hóa chất đầy đủ
    /// </summary>
    OK = 0,
    
    /// <summary>
    /// Hóa chất sắp hết
    /// </summary>
    Low = 1,
    
    /// <summary>
    /// Hóa chất đã hết
    /// </summary>
    Out = 2
}
