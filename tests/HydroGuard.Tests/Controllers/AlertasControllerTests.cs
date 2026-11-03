namespace HydroGuard.Tests.Controllers;

/// <summary>
/// Testes unitários do AlertasController.
/// Valida detecção de vazamentos.
/// </summary>
public class AlertasControllerTests : IntegrationTestBase
{
  public AlertasControllerTests(HydroGuardWebApplicationFactory factory) : base(factory) { }

  /// <summary>
  /// Teste 1: GET /api/alertas/vazamentos retorna status 200.
  /// </summary>
  [Fact]
  public async Task GetVazamentos_ReturnsHttpStatusCode200()
  {
    var response = await Client.GetAsync("/api/alertas/vazamentos");
    response.EnsureSuccessStatusCode();
    Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
  }

  /// <summary>
  /// Teste 2: GET vazamentos retorna lista (array JSON).
  /// </summary>
  [Fact]
  public async Task GetVazamentos_ReturnsList()
  {
    var response = await Client.GetAsync("/api/alertas/vazamentos");
    response.EnsureSuccessStatusCode();

    var content = await response.Content.ReadAsStringAsync();
    // Resposta deve ser um array JSON
    Assert.StartsWith("[", content.Trim());
  }

  /// <summary>
  /// Teste 3: GET vazamentos sem autenticação retorna 401.
  /// </summary>
  [Fact]
  public async Task GetVazamentos_WithoutAuth_ReturnsUnauthorized()
  {
    var client = Factory.CreateClient(); // Client sem token
    var response = await client.GetAsync("/api/alertas/vazamentos");
    Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
  }
}
