using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeManager.Backend.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUniquenessInETAndPF : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_PayFrequency_Name",
                table: "PayFrequency",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeType_Name",
                table: "EmployeeType",
                column: "Name",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PayFrequency_Name",
                table: "PayFrequency");

            migrationBuilder.DropIndex(
                name: "IX_EmployeeType_Name",
                table: "EmployeeType");
        }
    }
}
