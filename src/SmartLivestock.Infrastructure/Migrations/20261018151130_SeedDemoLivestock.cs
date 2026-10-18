using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartLivestock.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedDemoLivestock : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsDemo",
                table: "Animals",
                type: "bit",
                nullable: false,
                defaultValue: false);

            // Two clearly labeled sample animals per preloaded barn. Existing animals and
            // changed barn capacities are respected when this migration runs.
            string[] species = ["cattle", "buffalo", "pig", "chicken", "duck", "muscovy_duck",
                "goose", "goat", "sheep", "rabbit", "quail", "pigeon"];
            for (var barnNumber = 1; barnNumber <= 20; barnNumber++)
            {
                var barnId = $"c15308f0-224b-4dd2-9aba-764ea7a2f{barnNumber:000}";
                var firstId = $"a6d0f1e1-0000-4000-8000-{barnNumber:000000}000001";
                var secondId = $"a6d0f1e1-0000-4000-8000-{barnNumber:000000}000002";
                var firstTag = $"DEMO-{barnNumber:00}-01";
                var secondTag = $"DEMO-{barnNumber:00}-02";
                var code = species[(barnNumber - 1) % species.Length];
                migrationBuilder.Sql($"""
                    INSERT INTO [Animals] ([Id], [FarmId], [BarnId], [Tag], [Name], [Species], [RegisteredAt], [IsDemo])
                    SELECT v.[Id], b.[FarmId], b.[Id], v.[Tag], N'', N'{code}',
                           CONVERT(datetimeoffset, '2026-10-18T00:00:00+00:00'), CAST(1 AS bit)
                    FROM [Barns] AS b
                    CROSS APPLY (VALUES
                        (CONVERT(uniqueidentifier, '{firstId}'), N'{firstTag}'),
                        (CONVERT(uniqueidentifier, '{secondId}'), N'{secondTag}')) AS v([Id], [Tag])
                    WHERE b.[Id] = CONVERT(uniqueidentifier, '{barnId}')
                      AND b.[Capacity] - (SELECT COUNT(*) FROM [Animals] a WHERE a.[BarnId] = b.[Id]) >= 2
                      AND NOT EXISTS (SELECT 1 FROM [Animals] a WHERE a.[Id] IN
                          (CONVERT(uniqueidentifier, '{firstId}'), CONVERT(uniqueidentifier, '{secondId}'))
                          OR (a.[FarmId] = b.[FarmId] AND a.[Tag] IN (N'{firstTag}', N'{secondTag}')));
                    """);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            for (var barnNumber = 1; barnNumber <= 20; barnNumber++)
            {
                var firstId = $"a6d0f1e1-0000-4000-8000-{barnNumber:000000}000001";
                var secondId = $"a6d0f1e1-0000-4000-8000-{barnNumber:000000}000002";
                migrationBuilder.Sql($"""
                    DELETE FROM [Animals] WHERE [IsDemo] = 1 AND [Id] IN
                        (CONVERT(uniqueidentifier, '{firstId}'), CONVERT(uniqueidentifier, '{secondId}'));
                    """);
            }
            migrationBuilder.DropColumn(
                name: "IsDemo",
                table: "Animals");
        }
    }
}
