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


GO

DROP TABLE IF EXISTS MedicalRecords;
DROP TABLE IF EXISTS Bookings;
DROP TABLE IF EXISTS Patients;
SELECT * FROM Patients

