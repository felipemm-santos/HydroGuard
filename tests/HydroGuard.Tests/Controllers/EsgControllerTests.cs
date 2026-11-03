namespace HydroGuard.Tests.Controllers;

/// <summary>
/// Testes unitários do EsgController.
/// Valida indicadores ESG do dashboard.
/// </summary>
public class EsgControllerTests : IntegrationTestBase
{
  public EsgControllerTests(HydroGuardWebApplicationFactory factory) : base(factory) { }

  /// <summary>
  /// Teste 1: GET /api/esg/indicadores retorna status 200.
  /// </summary>
  [Fact]
  public async Task GetIndicadores_ReturnsHttpStatusCode200()
  {
    var response = await Client.GetAsync("/api/esg/indicadores");
    response.EnsureSuccessStatusCode();
    Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
  }

  /// <summary>
  /// Teste 2: GET indicadores retorna score ESG.
  /// </summary>
  [Fact]
  public async Task GetIndicadores_ReturnsScoreESG()
  {
    var response = await Client.GetAsync("/api/esg/indicadores");
    response.EnsureSuccessStatusCode();

    var content = await response.Content.ReadAsStringAsync();
    Assert.Contains("scoreESG", content);
    Assert.Contains("consumoMensal", content);
  }

  /// <summary>
  /// Teste 3: GET indicadores retorna todos os campos obrigatórios.
  /// </summary>
  [Fact]
  public async Task GetIndicadores_ReturnsAllFields()
  {
    var response = await Client.GetAsync("/api/esg/indicadores");
    response.EnsureSuccessStatusCode();

    var content = await response.Content.ReadAsStringAsync();
    Assert.Contains("consumoMedioDiario", content);
    Assert.Contains("desperdicioEstimado", content);
    Assert.Contains("aguaEconomizada", content);
  }
}
