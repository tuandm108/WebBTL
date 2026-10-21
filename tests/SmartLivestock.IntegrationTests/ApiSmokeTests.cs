using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using System.Net;

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
                        "Host=localhost;Database=unused;Username=unused;Password=unused",
                    ["Auth:ApiKey"] = new string('x', 32)
                })));
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        var response = await client.GetAsync("/health/live");
        Assert.True(response.IsSuccessStatusCode);
    }

    [Fact]
    public async Task DataApiRequiresServerKey()
    {
        await using var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder => builder.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Database"] =
                        "Host=localhost;Database=unused;Username=unused;Password=unused",
                    ["Auth:ApiKey"] = new string('x', 32)
                })));
        using var client = factory.CreateClient();
        Assert.Equal(HttpStatusCode.Unauthorized, (await client.GetAsync("/api/species")).StatusCode);
        client.DefaultRequestHeaders.Add("X-SmartLivestock-Api-Key", new string('x', 32));
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/species")).StatusCode);
    }
}
