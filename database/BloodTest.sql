CREATE DATABASE PatientServiceDB;
GO 

USE PatientServiceDB; 
GO 
CREATE TABLE Patients (
    PatientId BIGINT PRIMARY KEY IDENTITY(1,1),
    FullName NVARCHAR(255) NOT NULL,
    DateOfBirth DATE NOT NULL,
    Gender NVARCHAR(10),
    BloodGroup NVARCHAR(5),
    PhoneNumber NVARCHAR(20),
    Address NVARCHAR(255),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);
GO
CREATE TRIGGER trg_Update_Patient
ON Patients
AFTER UPDATE
AS
BEGIN
    UPDATE Patients
    SET UpdatedAt = GETDATE()
    WHERE PatientId IN (SELECT DISTINCT PatientId FROM Inserted);
END
GO

INSERT INTO Patients (FullName, DateOfBirth, Gender, BloodGroup, PhoneNumber, Address)
VALUES 
(N'Nguyễn Văn A', '1990-05-12', N'Nam', 'O+', '0912345678', N'Hà Nội'),
(N'Trần Thị B', '1985-09-21', N'Nữ', 'A-', '0987654321', N'Hồ Chí Minh'),
(N'Lê Văn C', '2000-01-01', N'Nam', 'B+', '0909123456', N'Đà Nẵng');

SELECT * FROM Patients;


GO

DROP TABLE IF EXISTS MedicalRecords;
DROP TABLE IF EXISTS Bookings;
DROP TABLE IF EXISTS Patients;
SELECT * FROM Patients

