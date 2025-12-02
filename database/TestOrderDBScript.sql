USE master;
ALTER DATABASE TestOrderDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE TestOrderDB;

CREATE DATABASE TestOrderDB

use TestOrderDB

CREATE TABLE AuditLog
(
	AuditLogId UNIQUEIDENTIFIER Primary key,
	Action nvarchar(255),
	Description nvarchar(max),
	UserId UNIQUEIDENTIFIER,
);
CREATE TABLE TestBundle (
    BundleId INT IDENTITY(1,1) PRIMARY KEY,
    BundleName NVARCHAR(50),
    Description NVARCHAR(255),
    Price FLOAT,
    IsActive BIT,
);
CREATE TABLE TimeBlock
(
	TimeBlockId int identity(1,1) PRIMARY KEY,
	TimeBlock TIME Not null UNIQUE,
);

CREATE TABLE AppointmentSlot
(
	SlotId uniqueIdentifier not null PRIMARY KEY,
	AppointmentDate DATE NOT NULL,
	TimeBlockId int not null FOREIGN KEY REFERENCES TimeBlock(TimeBlockId),
	MaxBooking int NOT NULL DEFAULT 10,
	CONSTRAINT UQ_TimeSlot UNIQUE (AppointmentDate, TimeBlockId)
);





CREATE TABLE Booking
(
    BookingId UNIQUEIDENTIFIER NOT NULL PRIMARY KEY,
    PatientId UNIQUEIDENTIFIER,
    Status TINYINT,
    PatientName NVARCHAR(255),
    PatientPhone NVARCHAR(12),
    PatientEmail NVARCHAR(255),
    BookingCode NVARCHAR(50),
    AppointmentSlotId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES AppointmentSlot(SlotId),
    CreateAt DATETIME DEFAULT GETDATE(),
    CreatedBy NVARCHAR(255),
    RunDate DATE,
    RanBy NVARCHAR(30),
    BundleId INT FOREIGN KEY REFERENCES TestBundle(BundleId),
	TotalPrice float,
	CancelAt DATETIME
);


CREATE TABLE PaymentEnvoice (
    PaymentNo INT IDENTITY(1,1) PRIMARY KEY,
    BookingId UNIQUEIDENTIFIER NOT NULL,
    Method NVARCHAR(50),
    Amount FLOAT,
    Status TINYINT,
    CreatedAt DATETIME DEFAULT GETDATE(),
    PaidAt DATETIME,
    Token NVARCHAR(max),
    FOREIGN KEY (BookingId) REFERENCES Booking(BookingId)
);

CREATE TABLE Comment
(
	CommentId bigint Identity(1,1) PRIMARY KEY,
	TestId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES Booking(BookingId),
	Comment nvarchar(1000),
	CommentDate Date,
)

Create Table TestCatalog
(
	CatalogId int identity(1,1) Primary KEY,
	TestName nvarchar(100) not null,
	Description nvarchar(255),
	Price float not null,
)

CREATE TABLE CatalogBundle (
    BundleId INT ,
    CatalogId INT ,
    SortOrder INT,
    PRIMARY KEY (BundleId, CatalogId),
    FOREIGN KEY (BundleId) REFERENCES TestBundle(BundleId),
    FOREIGN KEY (CatalogId) REFERENCES TestCatalog(CatalogId)
);

CREATE TABLE TestParameter(
	ParameterId Int Identity(1,1) PRIMARY KEY,
	ParameterName nvarchar(100) not null,
	Unit nvarchar(50),
	ReferenceRange nvarchar(100),
	MinRange float,
	MaxRange float,
)

CREATE TABLE CatalogParameter (
    CatalogId INT NOT NULL,
    ParameterId INT NOT NULL,
    PRIMARY KEY (CatalogId, ParameterId),
    FOREIGN KEY (CatalogId) REFERENCES TestCatalog(CatalogId),
    FOREIGN KEY (ParameterId) REFERENCES TestParameter(ParameterId)
);

Create Table BookingTest
(
	TestBookingNo bigint identity(1,1) Primary KEY,
	BookingId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES Booking(BookingId),
	CatalogId int FOREIGN KEY REFERENCES TestCatalog(CatalogId)
)

CREATE TABLE TestResult
(
	ResultId bigint Primary KEY Identity(1,1),
	TestBookingNo bigint FOREIGN KEY REFERENCES BookingTest(TestBookingNo),
	ParameterId int FOREIGN KEY REFERENCES TestParameter(ParameterId),
	ResultValue nvarchar(100),
	isNormal bit
)


CREATE TABLE TestReport
(
	DocumentId Bigint identity(1,1) PRIMARY KEY,
	BookingId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES Booking(BookingId),
	Filename nvarchar(255) not null,
	FileType nvarchar(max),
	ResultData VARBINARY(MAX),
	CreatedAt date DEFAULT GETDATE(),
)




------------------------------------------------------------
-- INITIAL DATA
------------------------------------------------------------

INSERT INTO TimeBlock (TimeBlock) VALUES
('8:00'),
('9:00'),
('10:00'),
('11:00'),
('13:00'),
('14:00'),
('15:00'),
('16:00');

INSERT INTO TestCatalog (TestName, Description, Price) VALUES
(N'Tổng phân tích tế bào máu (CBC)', N'Đánh giá toàn bộ tế bào máu', 120000),
(N'Xét nghiệm đường huyết', N'Đánh giá nồng độ glucose trong máu', 40000),
(N'Xét nghiệm mỡ máu', N'Đánh giá nồng độ lipid trong máu', 150000),
(N'Xét nghiệm chức năng gan', N'Đánh giá hoạt động của gan qua men gan và protein', 180000),
(N'Xét nghiệm chức năng thận', N'Đánh giá khả năng lọc của thận', 100000),
(N'TSH', N'Định lượng hormone kích thích tuyến giáp (TSH)', 80000),
(N'Estradiol', N'Định lượng hormone estradiol', 80000),
(N'Prolactin', N'Định lượng hormone prolactin', 80000),
(N'CRP', N'Định lượng protein phản ứng C (CRP)', 55000),
(N'RF', N'Định lượng yếu tố dạng thấp (RF)', 50000),
(N'Điện giải đồ', N'Đo các chất điện giải: Natri, Kali, Clo', 90000),          -- 11
(N'Xét nghiệm tuyến giáp toàn phần (FT3, FT4)', N'Đánh giá hormone FT3 và FT4', 140000),  -- 12
(N'Xét nghiệm HbA1c', N'Đánh giá đường huyết trung bình 3 tháng', 120000),     -- 13
(N'Xét nghiệm viêm gan B (HBsAg)', N'Tầm soát kháng nguyên viêm gan B', 80000), -- 14
(N'Xét nghiệm viêm gan C (Anti-HCV)', N'Tầm soát kháng thể HCV', 90000),       -- 15
(N'Xét nghiệm nước tiểu 10 thông số', N'Đánh giá thận và chuyển hóa', 60000),  -- 16
(N'Xét nghiệm acid uric', N'Đo nồng độ acid uric trong máu', 70000),           -- 17
(N'Xét nghiệm amylase', N'Đo hoạt độ men amylase', 80000),                     -- 18
(N'Xét nghiệm lipase', N'Đo hoạt độ men lipase', 85000),                       -- 19
(N'Xét nghiệm Beta-hCG', N'Định lượng hormone hCG', 100000); 


INSERT INTO TestParameter (ParameterName, Unit, ReferenceRange, MinRange, MaxRange) VALUES
-- CBC
(N'Hồng cầu (RBC)', N'10^6/mcL', N'4.0 - 5.5', 4.0, 5.5),
(N'Hemoglobin (Hb)', N'g/dL', N'12.0 - 16.0', 12.0, 16.0),
(N'Hematocrit (Hct)', N'%', N'38 - 46', 38, 46),
(N'Bạch cầu (WBC)', N'10^3/mcL', N'4.0 - 10.0', 4.0, 10.0),
(N'Tiểu cầu (PLT)', N'10^3/mcL', N'150 - 400', 150, 400),

-- Đường huyết
(N'Đường huyết (Glucose)', N'mg/dL', N'70 - 100', 70, 100),

-- Mỡ máu
(N'Cholesterol toàn phần', N'mg/dL', N'125 - 200', 125, 200),
(N'LDL cholesterol', N'mg/dL', N'0 - 100', 0, 100),
(N'HDL cholesterol', N'mg/dL', N'40 - 60', 40, 60),
(N'Triglyceride', N'mg/dL', N'30 - 150', 30, 150),

-- Chức năng gan
(N'ALT (SGPT)', N'U/L', N'7 - 55', 7, 55),
(N'AST (SGOT)', N'U/L', N'8 - 48', 8, 48),
(N'ALP', N'U/L', N'40 - 129', 40, 129),
(N'Albumin', N'g/dL', N'3.5 - 5.0', 3.5, 5.0),
(N'Bilirubin toàn phần', N'mg/dL', N'0.1 - 1.2', 0.1, 1.2),

-- Chức năng thận
(N'Creatinine', N'mg/dL', N'0.6 - 1.3', 0.6, 1.3),
(N'Ure (BUN)', N'mg/dL', N'7 - 20', 7, 20),

-- Nội tiết & Miễn dịch
(N'TSH', N'mIU/L', N'0.4 - 5.0', 0.4, 5.0),
(N'Estradiol', N'pmol/L', N'70 - 220', 70, 220),
(N'Prolactin', N'mU/L', N'127 - 637', 127, 637),
(N'CRP', N'mg/L', N'0 - 10', 0, 10),
(N'RF', N'IU/mL', N'0 - 14', 0, 14),

(N'Natri (Na+)', N'mmol/L', N'135 - 145', 135, 145),          -- 23
(N'Kali (K+)', N'mmol/L', N'3.5 - 5.0', 3.5, 5.0),            -- 24
(N'Clo (Cl-)', N'mmol/L', N'96 - 106', 96, 106),              -- 25

-- FT3 - FT4 (Catalog 12)
(N'FT3', N'pg/mL', N'2.0 - 4.4', 2.0, 4.4),                    -- 26
(N'FT4', N'ng/dL', N'0.8 - 1.8', 0.8, 1.8),                    -- 27

-- HbA1c (Catalog 13)
(N'HbA1c', N'%', N'4.0 - 5.6', 4.0, 5.6),                      -- 28

-- HBsAg (Catalog 14)
(N'HBsAg định tính', N'', N'Âm tính', NULL, NULL),            -- 29

-- Anti-HCV (Catalog 15)
(N'Anti-HCV định tính', N'', N'Âm tính', NULL, NULL),         -- 30

-- Nước tiểu 10 thông số (Catalog 16)
(N'pH nước tiểu', N'', N'4.5 - 8.0', 4.5, 8.0),               -- 31
(N'Protein niệu', N'mg/dL', N'0 - 20', 0, 20),                -- 32
(N'Glucose niệu', N'mg/dL', N'0 - 15', 0, 15),                -- 33
(N'Ketone', N'mg/dL', N'0 - 5', 0, 5),                        -- 34
(N'Bilirubin niệu', N'mg/dL', N'0 - 0.3', 0, 0.3),            -- 35

-- Acid uric (Catalog 17)
(N'Acid uric', N'mg/dL', N'3.5 - 7.2', 3.5, 7.2),             -- 36

-- Amylase (Catalog 18)
(N'Amylase', N'U/L', N'30 - 110', 30, 110),                   -- 37

-- Lipase (Catalog 19)
(N'Lipase', N'U/L', N'13 - 60', 13, 60),                      -- 38

-- Beta-hCG (Catalog 20)
(N'Beta-hCG', N'mIU/mL', N'0 - 25', 0, 25);                   -- 39


INSERT INTO CatalogParameter (CatalogId, ParameterId) VALUES
-- CBC
(1, 1), (1, 2), (1, 3), (1, 4), (1, 5),

-- Đường huyết
(2, 6),

-- Mỡ máu
(3, 7), (3, 8), (3, 9), (3, 10),

-- Chức năng gan
(4, 11), (4, 12), (4, 13), (4, 14), (4, 15),

-- Chức năng thận
(5, 16), (5, 17),

-- Nội tiết & miễn dịch
(6, 18),  -- TSH
(7, 19),  -- Estradiol
(8, 20),  -- Prolactin
(9, 21),  -- CRP
(10, 22), -- RF

-- 11: Điện giải đồ
(11, 23), (11, 24), (11, 25),

-- 12: FT3 - FT4
(12, 26), (12, 27),

-- 13: HbA1c
(13, 28),

-- 14: HBsAg
(14, 29),

-- 15: Anti-HCV
(15, 30),

-- 16: Nước tiểu 10 thông số
(16, 31), (16, 32), (16, 33), (16, 34), (16, 35),

-- 17: Acid uric
(17, 36),

-- 18: Amylase
(18, 37),

-- 19: Lipase
(19, 38),

-- 20: Beta-hCG
(20, 39);




INSERT INTO TestBundle (BundleName,Description,Price,IsActive)
VALUES
(N'Xét nghiệm tổng quát',N'Gói xét nghiệm cơ bản bao gồm công thức máu, đường huyết, chức năng gan',450000, 1),
(N'Xét nghiệm sinh hóa',N'Đánh giá chức năng gan, thận và các chỉ số sinh hóa quan trọng',650000, 1),
(N'Xét nghiệm toàn diện',N'Gói xét nghiệm đầy đủ nhất cho sức khỏe tổng thể',1200000, 1)


-- ========================
-- Bundle 1: Xét nghiệm tổng quát (Id = 1)
-- ========================
INSERT INTO CatalogBundle (BundleId, CatalogId) VALUES
(1, 1),   -- CBC
(1, 2),   -- Đường huyết
(1, 4),   -- Chức năng gan
(1, 11),  -- Điện giải đồ
(1, 13);  -- HbA1c


-- ========================
-- Bundle 2: Xét nghiệm sinh hóa (Id = 2)
-- ========================
INSERT INTO CatalogBundle (BundleId, CatalogId) VALUES
(2, 3),   -- Mỡ máu
(2, 5),   -- Chức năng thận
(2, 16),  -- Nước tiểu 10 thông số
(2, 17),  -- Acid uric
(2, 18),  -- Amylase
(2, 19);  -- Lipase


-- ========================
-- Bundle 3: Xét nghiệm toàn diện (Id = 3)
-- Gồm tất cả 20 catalog
-- ========================
INSERT INTO CatalogBundle (BundleId, CatalogId) VALUES
(3, 1), (3, 2), (3, 3), (3, 4), (3, 5),
(3, 6), (3, 7), (3, 8), (3, 9), (3, 10),
(3, 11), (3, 12), (3, 13), (3, 14), (3, 15),
(3, 16), (3, 17), (3, 18), (3, 19), (3, 20);

delete from Booking
		select * from Booking
		select * from CatalogBundle
		select * from TestCatalog
		select * from TestBundle
		select * from TestParameter