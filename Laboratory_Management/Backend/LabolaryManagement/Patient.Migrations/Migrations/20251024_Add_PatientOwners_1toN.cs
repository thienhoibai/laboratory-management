using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Patient.Infrastructure;

#nullable disable

namespace Patient.Migrations.Migrations
{
    [DbContext(typeof(PatientDbContext))]
    [Migration("202510240900_Add_PatientOwners_1toN")]
    public partial class Add_PatientOwners_1toN : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
IF OBJECT_ID('patient_owners','U') IS NULL
BEGIN
    CREATE TABLE patient_owners(
      patient_id UNIQUEIDENTIFIER NOT NULL,
      user_id    UNIQUEIDENTIFIER NOT NULL,
      created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
      CONSTRAINT PK_patient_owners PRIMARY KEY (patient_id, user_id),
      CONSTRAINT FK_patient_owners_patients FOREIGN KEY (patient_id)
        REFERENCES patients(patient_id) ON DELETE CASCADE
    );
    CREATE INDEX IX_patient_owners_user ON patient_owners(user_id, patient_id);
END
");

            // Seed from legacy patients.user_id
            migrationBuilder.Sql(@"
INSERT INTO patient_owners(patient_id, user_id, created_at)
SELECT patient_id, user_id, SYSUTCDATETIME()
FROM patients WITH (NOLOCK)
WHERE user_id IS NOT NULL
  AND NOT EXISTS (
        SELECT 1 FROM patient_owners po WHERE po.patient_id = patients.patient_id AND po.user_id = patients.user_id
  );
");

            // Make patients.user_id nullable (keep column for 1-2 versions, remove unique if any)
            migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name LIKE 'UX_%user_id%' AND object_id = OBJECT_ID('patients'))
BEGIN
    DECLARE @sql NVARCHAR(4000);
    SELECT TOP 1 @sql = 'DROP INDEX ' + QUOTENAME(i.name) + ' ON patients' FROM sys.indexes i WHERE i.object_id = OBJECT_ID('patients') AND i.name LIKE 'UX_%user_id%';
    EXEC sp_executesql @sql;
END
");
            migrationBuilder.AlterColumn<Guid>(
                name: "user_id",
                table: "patients",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Do not try to restore unique constraint; just drop link table
            migrationBuilder.Sql("IF OBJECT_ID('patient_owners','U') IS NOT NULL DROP TABLE patient_owners;");
        }
    }
}
