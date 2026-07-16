using System.Net;

namespace Locatic.Tests.Integration;

public class HealthEndpointTests : IClassFixture<LocaticWebApplicationFactory>
{
    private readonly HttpClient _client;

    public HealthEndpointTests(LocaticWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Health_RenvoieOk()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal("Healthy", body);
    }

    [Fact]
    public async Task HealthReady_RenvoieOk_QuandSqliteAccessible()
    {
        var response = await _client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal("Healthy", body);
    }

    [Fact]
    public async Task Metrics_ExposePrometheus()
    {
        var response = await _client.GetAsync("/metrics");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("#", body);
    }
}
