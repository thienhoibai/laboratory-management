-- ========================================
-- SCRIPT SỬA LỖI DATABASE CHO BOOKING TABLE
-- ========================================
USE TestOder;
GO

-- 1. Kiểm tra và thêm cột PatientEmail nếu chưa có
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Booking' AND COLUMN_NAME = 'PatientEmail'
)
BEGIN
    ALTER TABLE Booking ADD PatientEmail NVARCHAR(255) NULL;
    PRINT '✅ Đã thêm cột PatientEmail';
END
ELSE
BEGIN
    PRINT '✓ Cột PatientEmail đã tồn tại';
END
GO

-- 2. Kiểm tra và thêm cột TotalPrice nếu chưa có
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Booking' AND COLUMN_NAME = 'TotalPrice'
)
BEGIN
    ALTER TABLE Booking ADD TotalPrice FLOAT NULL;
    PRINT '✅ Đã thêm cột TotalPrice';
END
ELSE
BEGIN
    PRINT '✓ Cột TotalPrice đã tồn tại';
END
GO

-- 3. Kiểm tra và thêm cột CreateTime nếu chưa có
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'Booking' AND COLUMN_NAME = 'CreateTime'
)
BEGIN
    ALTER TABLE Booking ADD CreateTime TIME NULL DEFAULT GETDATE();
    PRINT '✅ Đã thêm cột CreateTime';
END
ELSE
BEGIN
    PRINT '✓ Cột CreateTime đã tồn tại';
END
GO

-- 4. Kiểm tra và thêm cột Token vào PaymentEnvoice nếu chưa có
IF NOT EXISTS (
    SELECT * FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'PaymentEnvoice' AND COLUMN_NAME = 'Token'
)
BEGIN
    ALTER TABLE PaymentEnvoice ADD Token NVARCHAR(100) NULL;
    PRINT '✅ Đã thêm cột Token vào PaymentEnvoice';
END
ELSE
BEGIN
    PRINT '✓ Cột Token đã tồn tại trong PaymentEnvoice';
END
GO

-- 5. Hiển thị cấu trúc bảng Booking sau khi cập nhật
PRINT '';
PRINT '📊 CẤU TRÚC BẢNG BOOKING:';
SELECT 
    COLUMN_NAME, 
    DATA_TYPE, 
    CHARACTER_MAXIMUM_LENGTH, 
    IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Booking'
ORDER BY ORDINAL_POSITION;
GO

PRINT '';
PRINT '✅ HOÀN THÀNH! Database đã sẵn sàng.';
