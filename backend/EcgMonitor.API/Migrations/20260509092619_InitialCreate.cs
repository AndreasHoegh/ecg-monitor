using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace EcgMonitor.API.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "EcgRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    PatientId = table.Column<string>(type: "text", nullable: false),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    SampleRateHz = table.Column<int>(type: "integer", nullable: false),
                    DataPoints = table.Column<double[]>(type: "double precision[]", nullable: false),
                    HeartRateBpm = table.Column<double>(type: "double precision", nullable: false),
                    IsAnomaly = table.Column<bool>(type: "boolean", nullable: false),
                    AiDiagnosis = table.Column<string>(type: "text", nullable: true),
                    AiReasoning = table.Column<string>(type: "text", nullable: true),
                    AiConfidence = table.Column<double>(type: "double precision", nullable: true),
                    AiUrgency = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EcgRecords", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DoctorReviews",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    EcgRecordId = table.Column<Guid>(type: "uuid", nullable: false),
                    DoctorName = table.Column<string>(type: "text", nullable: false),
                    Diagnosis = table.Column<string>(type: "text", nullable: false),
                    Notes = table.Column<string>(type: "text", nullable: false),
                    AgreedWithAi = table.Column<bool>(type: "boolean", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DoctorReviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DoctorReviews_EcgRecords_EcgRecordId",
                        column: x => x.EcgRecordId,
                        principalTable: "EcgRecords",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DoctorReviews_EcgRecordId",
                table: "DoctorReviews",
                column: "EcgRecordId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DoctorReviews");

            migrationBuilder.DropTable(
                name: "EcgRecords");
        }
    }
}
