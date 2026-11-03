namespace HydroGuard.Tests.Controllers;

/// <summary>
/// Testes unitários do RelatoriosController.
/// Valida relatório gerencial de consumo.
/// </summary>
public class RelatoriosControllerTests : IntegrationTestBase
{
  public RelatoriosControllerTests(HydroGuardWebApplicationFactory factory) : base(factory) { }

  /// <summary>
  /// Teste 1: GET /api/relatorios/consumo retorna status 200.
  /// </summary>
  [Fact]
  public async Task GetConsumo_ReturnsHttpStatusCode200()
  {
    var response = await Client.GetAsync("/api/relatorios/consumo");
    response.EnsureSuccessStatusCode();
    Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
  }

  /// <summary>
  /// Teste 2: GET relatório retorna top medidores.
  /// </summary>
  [Fact]
  public async Task GetConsumo_ReturnsTopMedidores()
  {
    var response = await Client.GetAsync("/api/relatorios/consumo");
    response.EnsureSuccessStatusCode();

    var content = await response.Content.ReadAsStringAsync();
    Assert.Contains("topMedidores", content);
    Assert.Contains("mediaPorPeriodo", content);
  }

  /// <summary>
  /// Teste 3: GET relatório retorna projeção futura.
  /// </summary>
  [Fact]
  public async Task GetConsumo_ReturnsProjecaoFutura()
  {
    var response = await Client.GetAsync("/api/relatorios/consumo");
    response.EnsureSuccessStatusCode();

    var content = await response.Content.ReadAsStringAsync();
    Assert.Contains("projecaoFutura", content);
    Assert.Contains("alertasCriticos", content);
  }
}
