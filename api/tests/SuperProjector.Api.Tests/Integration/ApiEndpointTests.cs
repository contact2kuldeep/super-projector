using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using SuperProjector.Api.Domain;

namespace SuperProjector.Api.Tests.Integration;

/// <summary>
/// Integration tests: WebApplicationFactory starts the real API in memory
/// (same Program.cs, routing, config and JSON handling) and gives us an
/// HttpClient to call it. No network port is opened.
/// </summary>
/// <remarks>
/// IClassFixture shares one running app across all tests in this class,
/// which is much faster than starting a new one per test.
/// </remarks>
public class ApiEndpointTests(WebApplicationFactory<Program> factory)
    : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client = factory.CreateClient();

    [Fact]
    public async Task GetHealth_AppRunning_ReturnsOkAndHealthy()
    {
        var response = await _client.GetAsync("/health");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<HealthBody>();
        Assert.Equal("Healthy", body?.Status);
    }

    [Fact]
    public async Task PostProjection_ValidRequest_ReturnsOkWithDisclaimer()
    {
        var request = new ProjectionRequest(40, 150_000m, 110_000m, 12m);

        var response = await _client.PostAsJsonAsync("/api/projection", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadFromJsonAsync<ProjectionResponse>();
        Assert.NotNull(body);
        Assert.Equal("Illustrative only. Not financial advice.", body.Disclaimer);
        // Proves appsettings.json was loaded and the full pipeline ran.
        Assert.Equal(1_932_437.34m, body.ProjectedBalance);
    }

    [Fact]
    public async Task PostProjection_Age70_ReturnsBadRequestWithAgeError()
    {
        var request = new ProjectionRequest(70, 150_000m, 110_000m, 12m);

        var response = await _client.PostAsJsonAsync("/api/projection", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.Contains("age", problem.Errors.Keys);
    }

    // Shape of the /health JSON body.
    private record HealthBody(string Status);
}
