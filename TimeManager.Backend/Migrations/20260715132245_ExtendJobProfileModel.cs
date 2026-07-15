using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeManager.Backend.Migrations
{
    /// <inheritdoc />
    public partial class ExtendJobProfileModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EarlyBuffer",
                table: "JobProfile",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndDate",
                table: "JobProfile",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "JoinDate",
                table: "JobProfile",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EarlyBuffer",
                table: "JobProfile");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "JobProfile");

            migrationBuilder.DropColumn(
                name: "JoinDate",
                table: "JobProfile");
        }
    }
}
