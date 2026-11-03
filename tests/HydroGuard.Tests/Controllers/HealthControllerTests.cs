using System.Net;
namespace HydroGuard.Tests.Controllers;
public class HealthControllerTests : IClassFixture<HydroGuardWebApplicationFactory>
{
  private readonly HttpClient _client;
  public HealthControllerTests(HydroGuardWebApplicationFactory factory) => _client = factory.CreateClient();
  [Fact]
  public async Task Readiness_ReturnsTestingEnvironmentWithoutAuthentication()
  {
    var response = await _client.GetAsync("/healthz");
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    var body = await response.Content.ReadAsStringAsync();
    Assert.Contains("healthy", body);
    Assert.Contains("Testing", body);
  }
  [Fact]
  public async Task Liveness_RespondsWithoutAuthentication()
  {
    var response = await _client.GetAsync("/livez");
    Assert.Equal(HttpStatusCode.OK, response.StatusCode);
  }
}
