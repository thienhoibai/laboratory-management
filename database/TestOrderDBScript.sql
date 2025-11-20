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
    CreateDate DATE DEFAULT GETDATE(),
    CreatedBy NVARCHAR(255),
    RunDate DATE,
    RanBy NVARCHAR(30),
    BundleId INT FOREIGN KEY REFERENCES TestBundle(BundleId)
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

-- Time blocks
INSERT INTO TimeBlock (TimeBlock)
VALUES ('07:00'), ('08:00'), ('09:00'), ('10:00'),
       ('11:00'), ('13:00'), ('14:00'), ('15:00'), ('16:00');
GO

------------------------------------------------------------
-- TestCatalog (20 tests)
------------------------------------------------------------
INSERT INTO TestCatalog (TestName, Description, Price)
VALUES
(N'Complete Blood Count', N'Xét nghiệm tổng phân tích tế bào máu', 500000),
(N'Lipid Panels', N'Đo mỡ máu, cholesterol và triglyceride', 400000),
(N'Blood Glucose Test', N'Kiểm tra lượng đường trong máu', 150000),
(N'Liver Function Test', N'Đánh giá chức năng gan', 300000),
(N'Kidney Function Test', N'Đánh giá chức năng thận', 250000),
(N'Electrolyte Panel', N'Đo nồng độ các chất điện giải trong máu', 200000),
(N'Thyroid Function Test', N'Đánh giá hormone tuyến giáp', 350000),
(N'Iron Studies', N'Đánh giá tình trạng sắt trong cơ thể', 220000),
(N'Urinalysis', N'Phân tích nước tiểu cơ bản', 180000),
(N'HbA1C Test', N'Đo chỉ số đường huyết trung bình trong 3 tháng', 270000),
(N'Hepatitis B Surface Antigen', N'Tầm soát viêm gan B', 180000),
(N'Hepatitis C Antibody', N'Tầm soát viêm gan C', 180000),
(N'HIV Screening Test', N'Tầm soát HIV', 250000),
(N'Dengue NS1 Antigen', N'Tầm soát sốt xuất huyết Dengue', 220000),
(N'Malaria Parasite Test', N'Kiểm tra ký sinh trùng sốt rét', 210000),
(N'COVID-19 RT-PCR', N'Xét nghiệm xác định SARS-CoV-2', 400000),
(N'Vitamin D Test', N'Đo nồng độ vitamin D trong máu', 280000),
(N'Calcium Test', N'Kiểm tra nồng độ canxi trong máu', 160000),
(N'Uric Acid Test', N'Đo nồng độ axit uric trong máu', 150000),
(N'Cholesterol Total Test', N'Kiểm tra tổng lượng cholesterol', 150000);
GO

------------------------------------------------------------
-- TestParameter (23 parameters)
------------------------------------------------------------
INSERT INTO TestParameter (ParameterName, ReferenceRange, Unit, MinRange, MaxRange)
VALUES
(N'Hồng cầu (RBC)', 'Nam: 4.2 - 6.0 | Nữ: 3.8 - 5.0', '*10^12/L', 3.8, 6.0),
(N'Hemoglobin (Hb)', 'Nam: 13.5 - 17.5 | Nữ: 12 - 16', 'g/dL', 12.0, 17.5),
(N'Hematocrit (HCT)', 'Nam: 40 - 52 | Nữ: 37 - 48', '%', 37, 52),
(N'Glucose', '70 - 100', 'mg/dL', 70, 100),
(N'AST (SGOT)', '10 - 40', 'U/L', 10, 40),
(N'ALT (SGPT)', '7 - 56', 'U/L', 7, 56),
(N'Creatinine', '0.6 - 1.3', 'mg/dL', 0.6, 1.3),
(N'Urea (BUN)', '7 - 20', 'mg/dL', 7, 20),
(N'Sodium (Na+)', '135 - 145', 'mmol/L', 135, 145),
(N'Potassium (K+)', '3.5 - 5.1', 'mmol/L', 3.5, 5.1),
(N'Chloride (Cl-)', '98 - 107', 'mmol/L', 98, 107),
(N'TSH', '0.4 - 4.0', 'mIU/L', 0.4, 4.0),
(N'T3', '0.8 - 2.0', 'ng/mL', 0.8, 2.0),
(N'T4', '4.5 - 11.2', 'µg/dL', 4.5, 11.2),
(N'Iron', '60 - 170', 'µg/dL', 60, 170),
(N'Ferritin', '20 - 300', 'ng/mL', 20, 300),
(N'Uric Acid', '3.5 - 7.2', 'mg/dL', 3.5, 7.2),
(N'Calcium', '8.5 - 10.2', 'mg/dL', 8.5, 10.2),
(N'Vitamin D (25-OH)', '30 - 100', 'ng/mL', 30, 100),
(N'Cholesterol Total', '<200', 'mg/dL', NULL, 200),
(N'Triglycerides', '<150', 'mg/dL', NULL, 150),
(N'HDL-C', '>40', 'mg/dL', 40, NULL),
(N'LDL-C', '<130', 'mg/dL', NULL, 130);
GO


------------------------------------------------------------
-- CatalogParameter mapping
------------------------------------------------------------
INSERT INTO CatalogParameter VALUES
(1,1),(1,2),(1,3),                     -- CBC
(2,20),(2,21),(2,22),(2,23),           -- Lipid Panel
(3,4),                                 -- Glucose
(4,5),(4,6),                           -- Liver
(5,7),(5,8),                           -- Kidney
(6,9),(6,10),(6,11),                   -- Electrolyte
(7,12),(7,13),(7,14),                  -- Thyroid
(8,15),(8,16),                         -- Iron
(9,4),(9,7),                           -- Urinalysis
(10,4),                                -- HbA1C
(17,19),                               -- Vitamin D
(18,18),                               -- Calcium
(19,17),                               -- Uric Acid
(20,20);                               -- Cholesterol
GO

INSERT INTO TestBundle (BundleName,Description,Price,IsActive)
VALUES
('Xét nghiệm tổng quát','Gói xét nghiệm cơ bản bao gồm công thức máu, đường huyết, chức năng gan',450000, 1),
('Xét nghiệm sinh hóa','Đánh giá chức năng gan, thận và các chỉ số sinh hóa quan trọng',650000, 1),
('Xét nghiệm toàn diện','Gói xét nghiệm đầy đủ nhất cho sức khỏe tổng thể',1200000, 1)
GO

INSERT INTO

ALTER TABLE Booking
ADD BookingCode NVARCHAR(10);
ALTER TABLE Booking
ADD PatientEmail nvarchar(255);


		select * from Booking
		select * from CatalogBundle
		select * from TestCatalog
		select * from TestBundle
		select * from TestParameter