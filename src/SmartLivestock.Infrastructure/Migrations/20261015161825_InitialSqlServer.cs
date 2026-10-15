using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartLivestock.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialSqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Farms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Farms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Barns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FarmId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Barns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Barns_Farms_FarmId",
                        column: x => x.FarmId,
                        principalTable: "Farms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Devices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BarnId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    LastSeenAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Devices_Barns_BarnId",
                        column: x => x.BarnId,
                        principalTable: "Barns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SensorReadings",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DeviceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MessageId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BootId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Sequence = table.Column<long>(type: "bigint", nullable: false),
                    SensorId = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Metric = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Value = table.Column<decimal>(type: "decimal(14,4)", precision: 14, scale: 4, nullable: false),
                    Unit = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    Quality = table.Column<string>(type: "nvarchar(16)", maxLength: 16, nullable: false),
                    MeasuredAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ReceivedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SensorReadings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SensorReadings_Devices_DeviceId",
                        column: x => x.DeviceId,
                        principalTable: "Devices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Farms",
                columns: new[] { "Id", "Code", "CreatedAt", "Name" },
                values: new object[] { new Guid("79f40d21-f916-42d2-a83a-16d5144b9f01"), "DEMO", new DateTimeOffset(new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)), "Trang trại mẫu" });

            migrationBuilder.InsertData(
                table: "Barns",
                columns: new[] { "Id", "Capacity", "Code", "FarmId", "Name" },
                values: new object[] { new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f001"), 100, "BARN-01", new Guid("79f40d21-f916-42d2-a83a-16d5144b9f01"), "Chuồng mẫu" });

            migrationBuilder.InsertData(
                table: "Devices",
                columns: new[] { "Id", "BarnId", "ExternalId", "LastSeenAt" },
                values: new object[] { new Guid("e996724d-0a30-461d-93d4-65190594d001"), new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f001"), "edge-001", null });

            migrationBuilder.CreateIndex(
                name: "IX_Barns_FarmId_Code",
                table: "Barns",
                columns: new[] { "FarmId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Devices_BarnId",
                table: "Devices",
                column: "BarnId");

            migrationBuilder.CreateIndex(
                name: "IX_Devices_ExternalId",
                table: "Devices",
                column: "ExternalId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Farms_Code",
                table: "Farms",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SensorReadings_DeviceId_MessageId_SensorId",
                table: "SensorReadings",
                columns: new[] { "DeviceId", "MessageId", "SensorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SensorReadings_DeviceId_Metric_MeasuredAt",
                table: "SensorReadings",
                columns: new[] { "DeviceId", "Metric", "MeasuredAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SensorReadings");

            migrationBuilder.DropTable(
                name: "Devices");

            migrationBuilder.DropTable(
                name: "Barns");

            migrationBuilder.DropTable(
                name: "Farms");
        }
    }
}
