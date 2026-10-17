using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartLivestock.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddBarnMonitored : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Monitored",
                table: "Barns",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.UpdateData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f001"),
                column: "Monitored",
                value: true);

            migrationBuilder.UpdateData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f002"),
                column: "Monitored",
                value: true);

            migrationBuilder.UpdateData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f003"),
                column: "Monitored",
                value: true);

            migrationBuilder.UpdateData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f004"),
                column: "Monitored",
                value: true);

            migrationBuilder.UpdateData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f005"),
                column: "Monitored",
                value: true);

            migrationBuilder.UpdateData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f006"),
                column: "Monitored",
                value: true);

            migrationBuilder.UpdateData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f007"),
                column: "Monitored",
                value: true);

            migrationBuilder.UpdateData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f008"),
                column: "Monitored",
                value: true);

            migrationBuilder.UpdateData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f009"),
                column: "Monitored",
                value: true);

            migrationBuilder.UpdateData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f010"),
                column: "Monitored",
                value: true);

            migrationBuilder.UpdateData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f011"),
                column: "Monitored",
                value: true);

            migrationBuilder.UpdateData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f012"),
                column: "Monitored",
                value: true);

            migrationBuilder.UpdateData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f013"),
                column: "Monitored",
                value: true);

            migrationBuilder.UpdateData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f014"),
                column: "Monitored",
                value: true);

            migrationBuilder.UpdateData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f015"),
                column: "Monitored",
                value: true);

            migrationBuilder.UpdateData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f016"),
                column: "Monitored",
                value: true);

            migrationBuilder.UpdateData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f017"),
                column: "Monitored",
                value: true);

            migrationBuilder.UpdateData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f018"),
                column: "Monitored",
                value: true);

            migrationBuilder.UpdateData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f019"),
                column: "Monitored",
                value: true);

            migrationBuilder.UpdateData(
                table: "Barns",
                keyColumn: "Id",
                keyValue: new Guid("c15308f0-224b-4dd2-9aba-764ea7a2f020"),
                column: "Monitored",
                value: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Monitored",
                table: "Barns");
        }
    }
}
