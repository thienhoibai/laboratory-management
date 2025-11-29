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
	ResultFile nvarchar(max) not null,
	CreatedAt date DEFAULT GETDATE(),
)



------------------------------------------------------------
-- INITIAL DATA
------------------------------------------------------------
INSERT INTO TestCatalog (CatalogId, TestName, Description, Price) VALUES
(1, 'Tổng phân tích tế bào máu (CBC)', 'Đánh giá toàn bộ tế bào máu', 120000),
(2, 'Xét nghiệm đường huyết', 'Đánh giá nồng độ glucose trong máu', 40000),
(3, 'Xét nghiệm mỡ máu', 'Đánh giá nồng độ lipid trong máu', 150000),
(4, 'Xét nghiệm chức năng gan', 'Đánh giá hoạt động của gan qua men gan và protein', 180000),
(5, 'Xét nghiệm chức năng thận', 'Đánh giá khả năng lọc của thận', 100000),
(6, 'TSH', 'Định lượng hormone kích thích tuyến giáp (TSH)', 80000),
(7, 'Estradiol', 'Định lượng hormone estradiol', 80000),
(8, 'Prolactin', 'Định lượng hormone prolactin', 80000),
(9, 'CRP', 'Định lượng protein phản ứng C (CRP)', 55000),
(10, 'RF', 'Định lượng yếu tố dạng thấp (RF)', 50000);



INSERT INTO TestParameter (ParameterId, ParameterName, Unit, ReferenceRange, MinRange, MaxRange) VALUES
-- CBC
(1, 'Hồng cầu (RBC)', '10^6/µL', '4.0 - 5.5', 4.0, 5.5),
(2, 'Hemoglobin (Hb)', 'g/dL', '12.0 - 16.0', 12.0, 16.0),
(3, 'Hematocrit (Hct)', '%', '38 - 46', 38, 46),
(4, 'Bạch cầu (WBC)', '10^3/µL', '4.0 - 10.0', 4.0, 10.0),
(5, 'Tiểu cầu (PLT)', '10^3/µL', '150 - 400', 150, 400),

-- Đường huyết
(6, 'Đường huyết (Glucose)', 'mg/dL', '70 - 100', 70, 100),

-- Mỡ máu
(7, 'Cholesterol toàn phần', 'mg/dL', '125 - 200', 125, 200),
(8, 'LDL cholesterol', 'mg/dL', '0 - 100', 0, 100),
(9, 'HDL cholesterol', 'mg/dL', '40 - 60', 40, 60),
(10, 'Triglyceride', 'mg/dL', '30 - 150', 30, 150),

-- Chức năng gan
(11, 'ALT (SGPT)', 'U/L', '7 - 55', 7, 55),
(12, 'AST (SGOT)', 'U/L', '8 - 48', 8, 48),
(13, 'ALP', 'U/L', '40 - 129', 40, 129),
(14, 'Albumin', 'g/dL', '3.5 - 5.0', 3.5, 5.0),
(15, 'Bilirubin toàn phần', 'mg/dL', '0.1 - 1.2', 0.1, 1.2),

-- Chức năng thận
(16, 'Creatinine', 'mg/dL', '0.6 - 1.3', 0.6, 1.3),
(17, 'Ure (BUN)', 'mg/dL', '7 - 20', 7, 20),

(18, 'TSH', 'mIU/L', '0.4-5.0', 0.4, 5.0),
(19, 'Estradiol', 'pmol/L', '70-220', 70, 220),
(20, 'Prolactin', 'μU/mL', '127-637', 127, 637),
(21, 'CRP', 'mg/L', '0-10', 0, 10),
(22, 'RF', 'IU/mL', '0-14', 0, 14);

INSERT INTO CatalogParameter (CatalogId, ParameterId) VALUES
-- CBC
(1,1),(1,2),(1,3),(1,4),(1,5),

-- Đường huyết
(2,6),

-- Mỡ máu
(3,7),(3,8),(3,9),(3,10),

-- Chức năng gan
(4,11),(4,12),(4,13),(4,14),(4,15),

-- Chức năng thận
(5,16),(5,17),

(6, 18),
(7, 19),
(8, 20),
(9, 21),
(10, 22);


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