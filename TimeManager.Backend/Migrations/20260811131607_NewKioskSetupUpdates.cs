using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeManager.Backend.Migrations
{
    /// <inheritdoc />
    public partial class NewKioskSetupUpdates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowedIPAddress",
                table: "Kiosk");

            migrationBuilder.AddColumn<string>(
                name: "DeviceToken",
                table: "Kiosk",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UniqueId",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeviceToken",
                table: "Kiosk");

            migrationBuilder.DropColumn(
                name: "UniqueId",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "AllowedIPAddress",
                table: "Kiosk",
                type: "nvarchar(45)",
                nullable: false,
                defaultValue: "");
        }
    }
}
