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
(N'RF', N'Định lượng yếu tố dạng thấp (RF)', 50000);


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
(N'RF', N'IU/mL', N'0 - 14', 0, 14);


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
(10, 22); -- RF



INSERT INTO TestBundle (BundleName,Description,Price,IsActive)
VALUES
(N'Xét nghiệm tổng quát',N'Gói xét nghiệm cơ bản bao gồm công thức máu, đường huyết, chức năng gan',450000, 1),
(N'Xét nghiệm sinh hóa',N'Đánh giá chức năng gan, thận và các chỉ số sinh hóa quan trọng',650000, 1),
(N'Xét nghiệm toàn diện',N'Gói xét nghiệm đầy đủ nhất cho sức khỏe tổng thể',1200000, 1)

delete from Booking
		select * from Booking
		select * from CatalogBundle
		select * from TestCatalog
		select * from TestBundle
		select * from TestParameter