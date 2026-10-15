using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace SmartLivestock.IntegrationTests;

public sealed class ApiSmokeTests
{
    [Fact]
    public async Task LivenessEndpointRespondsWithoutDatabaseConnection()
    {
        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Database"] =
                        "Host=localhost;Database=unused;Username=unused;Password=unused"
                })));
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var response = await client.GetAsync("/health/live");
        Assert.True(response.IsSuccessStatusCode);
    }
}
