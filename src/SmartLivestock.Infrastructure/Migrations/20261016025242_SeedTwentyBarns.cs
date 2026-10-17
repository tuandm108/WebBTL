using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace SmartLivestock.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedTwentyBarns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Barns",
                columns: new[] { "Id", "Capacity", "Code", "FarmId", "Name" },
                values: new object[,]
                {
                    { new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f002"), 100, "BARN-02", new Guid("79f40d21-f916-42d2-a83a-16d5144b9f01"), "Chuồng 02" },
                    { new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f003"), 120, "BARN-03", new Guid("79f40d21-f916-42d2-a83a-16d5144b9f01"), "Chuồng 03" },
                    { new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f004"), 140, "BARN-04", new Guid("79f40d21-f916-42d2-a83a-16d5144b9f01"), "Chuồng 04" },
                    { new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f005"), 60, "BARN-05", new Guid("79f40d21-f916-42d2-a83a-16d5144b9f01"), "Chuồng 05" },
                    { new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f006"), 80, "BARN-06", new Guid("79f40d21-f916-42d2-a83a-16d5144b9f01"), "Chuồng 06" },
                    { new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f007"), 100, "BARN-07", new Guid("79f40d21-f916-42d2-a83a-16d5144b9f01"), "Chuồng 07" },
                    { new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f008"), 120, "BARN-08", new Guid("79f40d21-f916-42d2-a83a-16d5144b9f01"), "Chuồng 08" },
                    { new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f009"), 140, "BARN-09", new Guid("79f40d21-f916-42d2-a83a-16d5144b9f01"), "Chuồng 09" },
                    { new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f010"), 60, "BARN-10", new Guid("79f40d21-f916-42d2-a83a-16d5144b9f01"), "Chuồng 10" },
                    { new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f011"), 80, "BARN-11", new Guid("79f40d21-f916-42d2-a83a-16d5144b9f01"), "Chuồng 11" },
                    { new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f012"), 100, "BARN-12", new Guid("79f40d21-f916-42d2-a83a-16d5144b9f01"), "Chuồng 12" },
                    { new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f013"), 120, "BARN-13", new Guid("79f40d21-f916-42d2-a83a-16d5144b9f01"), "Chuồng 13" },
                    { new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f014"), 140, "BARN-14", new Guid("79f40d21-f916-42d2-a83a-16d5144b9f01"), "Chuồng 14" },
                    { new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f015"), 60, "BARN-15", new Guid("79f40d21-f916-42d2-a83a-16d5144b9f01"), "Chuồng 15" },
                    { new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f016"), 80, "BARN-16", new Guid("79f40d21-f916-42d2-a83a-16d5144b9f01"), "Chuồng 16" },
                    { new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f017"), 100, "BARN-17", new Guid("79f40d21-f916-42d2-a83a-16d5144b9f01"), "Chuồng 17" },
                    { new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f018"), 120, "BARN-18", new Guid("79f40d21-f916-42d2-a83a-16d5144b9f01"), "Chuồng 18" },
                    { new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f019"), 140, "BARN-19", new Guid("79f40d21-f916-42d2-a83a-16d5144b9f01"), "Chuồng 19" },
                    { new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f020"), 60, "BARN-20", new Guid("79f40d21-f916-42d2-a83a-16d5144b9f01"), "Chuồng 20" }
                });

            migrationBuilder.InsertData(
                table: "Devices",
                columns: new[] { "Id", "BarnId", "ExternalId", "LastSeenAt" },
                values: new object[,]
                {
                    { new Guid("e996724d-0a30-461d-93d4-65190594d002"), new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f002"), "edge-002", null },
                    { new Guid("e996724d-0a30-461d-93d4-65190594d003"), new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f003"), "edge-003", null },
                    { new Guid("e996724d-0a30-461d-93d4-65190594d004"), new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f004"), "edge-004", null },
                    { new Guid("e996724d-0a30-461d-93d4-65190594d005"), new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f005"), "edge-005", null },
                    { new Guid("e996724d-0a30-461d-93d4-65190594d006"), new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f006"), "edge-006", null },
                    { new Guid("e996724d-0a30-461d-93d4-65190594d007"), new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f007"), "edge-007", null },
                    { new Guid("e996724d-0a30-461d-93d4-65190594d008"), new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f008"), "edge-008", null },
                    { new Guid("e996724d-0a30-461d-93d4-65190594d009"), new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f009"), "edge-009", null },
                    { new Guid("e996724d-0a30-461d-93d4-65190594d010"), new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f010"), "edge-010", null },
                    { new Guid("e996724d-0a30-461d-93d4-65190594d011"), new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f011"), "edge-011", null },
                    { new Guid("e996724d-0a30-461d-93d4-65190594d012"), new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f012"), "edge-012", null },
                    { new Guid("e996724d-0a30-461d-93d4-65190594d013"), new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f013"), "edge-013", null },
                    { new Guid("e996724d-0a30-461d-93d4-65190594d014"), new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f014"), "edge-014", null },
                    { new Guid("e996724d-0a30-461d-93d4-65190594d015"), new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f015"), "edge-015", null },
                    { new Guid("e996724d-0a30-461d-93d4-65190594d016"), new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f016"), "edge-016", null },
                    { new Guid("e996724d-0a30-461d-93d4-65190594d017"), new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f017"), "edge-017", null },
                    { new Guid("e996724d-0a30-461d-93d4-65190594d018"), new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f018"), "edge-018", null },
                    { new Guid("e996724d-0a30-461d-93d4-65190594d019"), new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f019"), "edge-019", null },
                    { new Guid("e996724d-0a30-461d-93d4-65190594d020"), new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f020"), "edge-020", null }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: new Guid("e996724d-0a30-461d-93d4-65190594d002"));

            migrationBuilder.DeleteData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: new Guid("e996724d-0a30-461d-93d4-65190594d003"));

            migrationBuilder.DeleteData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: new Guid("e996724d-0a30-461d-93d4-65190594d004"));

            migrationBuilder.DeleteData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: new Guid("e996724d-0a30-461d-93d4-65190594d005"));

            migrationBuilder.DeleteData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: new Guid("e996724d-0a30-461d-93d4-65190594d006"));

            migrationBuilder.DeleteData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: new Guid("e996724d-0a30-461d-93d4-65190594d007"));

            migrationBuilder.DeleteData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: new Guid("e996724d-0a30-461d-93d4-65190594d008"));

            migrationBuilder.DeleteData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: new Guid("e996724d-0a30-461d-93d4-65190594d009"));

            migrationBuilder.DeleteData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: new Guid("e996724d-0a30-461d-93d4-65190594d010"));

            migrationBuilder.DeleteData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: new Guid("e996724d-0a30-461d-93d4-65190594d011"));

            migrationBuilder.DeleteData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: new Guid("e996724d-0a30-461d-93d4-65190594d012"));

            migrationBuilder.DeleteData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: new Guid("e996724d-0a30-461d-93d4-65190594d013"));

            migrationBuilder.DeleteData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: new Guid("e996724d-0a30-461d-93d4-65190594d014"));

            migrationBuilder.DeleteData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: new Guid("e996724d-0a30-461d-93d4-65190594d015"));

            migrationBuilder.DeleteData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: new Guid("e996724d-0a30-461d-93d4-65190594d016"));

            migrationBuilder.DeleteData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: new Guid("e996724d-0a30-461d-93d4-65190594d017"));

            migrationBuilder.DeleteData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: new Guid("e996724d-0a30-461d-93d4-65190594d018"));

            migrationBuilder.DeleteData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: new Guid("e996724d-0a30-461d-93d4-65190594d019"));

            migrationBuilder.DeleteData(
                table: "Devices",
                keyColumn: "Id",
                keyValue: new Guid("e996724d-0a30-461d-93d4-65190594d020"));

            migrationBuilder.DeleteData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f002"));

            migrationBuilder.DeleteData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f003"));

            migrationBuilder.DeleteData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f004"));

            migrationBuilder.DeleteData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f005"));

            migrationBuilder.DeleteData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f006"));

            migrationBuilder.DeleteData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f007"));

            migrationBuilder.DeleteData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f008"));

            migrationBuilder.DeleteData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f009"));

            migrationBuilder.DeleteData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f010"));

            migrationBuilder.DeleteData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f011"));

            migrationBuilder.DeleteData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f012"));

            migrationBuilder.DeleteData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f013"));

            migrationBuilder.DeleteData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f014"));

            migrationBuilder.DeleteData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f015"));

            migrationBuilder.DeleteData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f016"));

            migrationBuilder.DeleteData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f017"));

            migrationBuilder.DeleteData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f018"));

            migrationBuilder.DeleteData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f019"));

            migrationBuilder.DeleteData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f020"));
        }
    }
}
