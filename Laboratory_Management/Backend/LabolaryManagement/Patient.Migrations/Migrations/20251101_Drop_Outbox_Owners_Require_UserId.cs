using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Patient.Infrastructure;

#nullable disable

namespace Patient.Migrations.Migrations
{
    [DbContext(typeof(PatientDbContext))]
    [Migration("20251101_Drop_Outbox_Owners_Require_UserId")]
    public partial class Drop_Outbox_Owners_Require_UserId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Drop outbox_messages if exists
            migrationBuilder.Sql(@"IF OBJECT_ID('outbox_messages','U') IS NOT NULL DROP TABLE outbox_messages;");

            // Drop patient_owners if exists
            migrationBuilder.Sql(@"IF OBJECT_ID('patient_owners','U') IS NOT NULL DROP TABLE patient_owners;");

            // Backfill null user_id with zero GUID to allow NOT NULL constraint
            migrationBuilder.Sql(@"IF EXISTS (SELECT 1 FROM sys.columns WHERE Name = 'user_id' AND Object_ID = OBJECT_ID('patients'))
                                   UPDATE patients SET user_id = '00000000-0000-0000-0000-000000000000' WHERE user_id IS NULL;");

            // Alter patients.user_id to NOT NULL
            migrationBuilder.AlterColumn<System.Guid>(
                name: "user_id",
                table: "patients",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(System.Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert patients.user_id nullable
            migrationBuilder.AlterColumn<System.Guid>(
                name: "user_id",
                table: "patients",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(System.Guid),
                oldType: "uniqueidentifier");

            // Recreate patient_owners minimal (optional)
            migrationBuilder.Sql(@"IF OBJECT_ID('patient_owners','U') IS NULL
                                   CREATE TABLE patient_owners(
                                       patient_id UNIQUEIDENTIFIER NOT NULL,
                                       user_id UNIQUEIDENTIFIER NOT NULL,
                                       created_at DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
                                       CONSTRAINT PK_patient_owners PRIMARY KEY (patient_id, user_id)
                                   );");
        }
    }
}
