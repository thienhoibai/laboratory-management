namespace Patient.Domain.Enums;

/// <summary>
/// Nhóm máu của bệnh nhân
/// </summary>
public enum BloodType : byte
    {
        /// <summary>
        /// Không xác định
        /// </summary>
        Unknown = 0,
    
        /// <summary>
        /// Nhóm máu A+
        /// </summary>
        APositive = 1,
    
        /// <summary>
        /// Nhóm máu A-
        /// </summary>
        ANegative = 2,
    
        /// <summary>
        /// Nhóm máu B+
        /// </summary>
        BPositive = 3,
    
        /// <summary>
        /// Nhóm máu B-
        /// </summary>
        BNegative = 4,
    
        /// <summary>
        /// Nhóm máu AB+
        /// </summary>
        ABPositive = 5,
    
        /// <summary>
        /// Nhóm máu AB-
        /// </summary>
        ABNegative = 6,
    
        /// <summary>
        /// Nhóm máu O+
        /// </summary>
        OPositive = 7,
    
        /// <summary>
        /// Nhóm máu O-
        /// </summary>
        ONegative = 8
}
