namespace HydroGuard.Tests.Controllers;

public class HealthControllerTests : IClassFixture<HydroGuardWebApplicationFactory>
{
  private readonly HttpClient client;

  public HealthControllerTests(HydroGuardWebApplicationFactory factory) => client = factory.CreateClient();

  [Fact]
  public async Task Readiness_ReturnsTestingEnvironmentWithoutAuthentication()
  {
    using var response = await client.GetAsync("/healthz");
    var body = await ContractAssert.ReadAsync(response, HttpStatusCode.OK, "healthz");
    Assert.Equal("healthy", body["status"]!.GetValue<string>());
    Assert.Equal("Testing", body["environment"]!.GetValue<string>());
  }

  [Fact]
  public async Task Liveness_RespondsWithoutAuthentication()
  {
    using var response = await client.GetAsync("/livez");
    var body = await ContractAssert.ReadAsync(response, HttpStatusCode.OK, "livez");
    Assert.Equal("alive", body["status"]!.GetValue<string>());
  }
}
