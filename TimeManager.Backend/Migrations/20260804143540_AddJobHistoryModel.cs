using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeManager.Backend.Migrations
{
    /// <inheritdoc />
    public partial class AddJobHistoryModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "JobProfile");

            migrationBuilder.DropColumn(
                name: "JoinDate",
                table: "JobProfile");

            migrationBuilder.CreateTable(
                name: "JobHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobProfileId = table.Column<int>(type: "int", nullable: false),
                    ProfileTemplateId = table.Column<int>(type: "int", nullable: false),
                    JoinDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_JobHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_JobHistory_JobProfile_JobProfileId",
                        column: x => x.JobProfileId,
                        principalTable: "JobProfile",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_JobHistory_ProfileTemplate_ProfileTemplateId",
                        column: x => x.ProfileTemplateId,
                        principalTable: "ProfileTemplate",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_JobHistory_JobProfileId",
                table: "JobHistory",
                column: "JobProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_JobHistory_ProfileTemplateId",
                table: "JobHistory",
                column: "ProfileTemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "JobHistory");

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
    }
}
