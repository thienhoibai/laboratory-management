CREATE DATABASE MedicalRecordServiceDB;
GO

USE MedicalRecordServiceDB;
GO

CREATE TABLE MedicalRecords (
    RecordId BIGINT PRIMARY KEY IDENTITY(1,1),
    PatientId BIGINT NOT NULL, 
    Diagnosis NVARCHAR(255),
    Treatment NVARCHAR(255),
    Notes NVARCHAR(500),
    CreatedAt DATETIME DEFAULT GETDATE(),
    UpdatedAt DATETIME DEFAULT GETDATE()
);
GO

CREATE TABLE TestHistories (
    TestId BIGINT PRIMARY KEY IDENTITY(1,1),
    RecordId BIGINT NOT NULL, 
    TestType NVARCHAR(255) NOT NULL,
    TestDate DATETIME DEFAULT GETDATE(),
    Result NVARCHAR(MAX), 
    CreatedAt DATETIME DEFAULT GETDATE(),
    CONSTRAINT FK_TestHistories_MedicalRecords FOREIGN KEY (RecordId) REFERENCES MedicalRecords(RecordId)
);
GO
ALTER TABLE MedicalRecords
ADD RecordName NVARCHAR(255);

SELECT * FROM MedicalRecords
SELECT * FROM TestHistories