using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Patient.Migrations.Migrations
{
    public partial class Add_IdLast4_Column : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "id_last4",
                table: "patients",
                type: "nchar(4)",
                maxLength: 4,
                nullable: true,
                fixedLength: true);

            migrationBuilder.CreateIndex(
                name: "IX_patients_id_last4",
                table: "patients",
                column: "id_last4");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_patients_id_last4",
                table: "patients");

            migrationBuilder.DropColumn(
                name: "id_last4",
                table: "patients");
        }
    }
}
