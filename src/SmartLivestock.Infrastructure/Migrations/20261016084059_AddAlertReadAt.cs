using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartLivestock.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAlertReadAt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "ReadAt",
                table: "Alerts",
                type: "datetimeoffset",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ReadAt",
                table: "Alerts");
        }
    }
}
