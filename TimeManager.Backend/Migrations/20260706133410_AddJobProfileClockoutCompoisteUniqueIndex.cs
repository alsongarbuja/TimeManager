using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeManager.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddJobProfileClockoutCompoisteUniqueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_PunchEntry_JobProfileId",
                table: "PunchEntry");

            migrationBuilder.CreateIndex(
                name: "UX_PunchEntry_JobProfileId_OpenPunch",
                table: "PunchEntry",
                column: "JobProfileId",
                unique: true,
                filter: "[ClockOut] IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_PunchEntry_JobProfileId_OpenPunch",
                table: "PunchEntry");

            migrationBuilder.CreateIndex(
                name: "IX_PunchEntry_JobProfileId",
                table: "PunchEntry",
                column: "JobProfileId");
        }
    }
}
