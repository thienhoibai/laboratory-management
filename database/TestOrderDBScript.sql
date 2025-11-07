USE master;
ALTER DATABASE TestOder SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
DROP DATABASE TestOder;

CREATE DATABASE TestOder

use TestOder


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
	BookingId UNIQUEIDENTIFIER not null PRIMARY KEY,
	PatientId bigint,
	Status tinyint,
	PatientName nvarchar(255),
	PatientPhone nvarchar(12),
	AppointmentSlotId UNIQUEIDENTIFIER FOREIGN KEY REFERENCES AppointmentSlot(SlotId),
	CreateDate Date DEFAULT GETDATE(),
	CreatedBy nvarchar(30),
	RunDate Date,
	RanBy nvarchar(30),
	BundleId int FOREIGN KEY REFERENCES TestBundle(BundleId)
);


CREATE TABLE PaymentEnvoice (
    PaymentNo INT IDENTITY(1,1) PRIMARY KEY,
    BookingId UNIQUEIDENTIFIER NOT NULL,
    Method NVARCHAR(50),
    Amount FLOAT,
    Status NVARCHAR(50),
    CreatedAt DATETIME DEFAULT GETDATE(),
    PaidAt DATETIME,
    Token NVARCHAR(100),
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
	ReferenceRange nvarchar(100)
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



Insert into TimeBlock (TimeBlock)
VALUES	('7:00'),
		('8:00'),
		('9:00'),
		('10:00'),
		('11:00'),
		('13:00'),
		('14:00'),
		('15:00'),
		('16:00')
Insert into TestCatalog (TestName, Description, Price)
Values	('Complete Blood Count', 'Descriptions',500000),
		('Lipid Panels', 'Descriptions2', 400000)

Insert into TestParameter(ParameterName, ReferenceRange, Unit)
Values (N'Hồng cầu (RBC)', 'Nam: 4.2 - 6.0| Nữ: 3.8-5.0', '*10^12/L'),
		(N'Hemoglobin (Hb)', 'Nam: 13.5-17.5|  Nữ: 13.5-17.5', 'g/dL'),
		(N'Hematocrit (HCT)', 'Nam: 40-52| Nữ: 37-48', '%')

ALTER TABLE Booking
ADD BookingCode NVARCHAR(10);


		select * from Booking
		select * from CatalogBundle
		select * from TestCatalog
		select * from TestBundle