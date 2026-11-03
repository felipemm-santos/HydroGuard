namespace HydroGuard.Tests.Controllers;

/// <summary>
/// Testes unitários do MedidoresController.
/// Valida listagem e cadastro de medidores.
/// </summary>
public class MedidoresControllerTests : IntegrationTestBase
{
  public MedidoresControllerTests(HydroGuardWebApplicationFactory factory) : base(factory) { }

  /// <summary>
  /// Teste 1: GET /api/medidores retorna status 200.
  /// </summary>
  [Fact]
  public async Task Get_ReturnsHttpStatusCode200()
  {
    var response = await Client.GetAsync("/api/medidores?page=1&pageSize=10");
    response.EnsureSuccessStatusCode();
    Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
  }

  /// <summary>
  /// Teste 2: GET retorna lista paginada com estrutura correta.
  /// </summary>
  [Fact]
  public async Task Get_ReturnsPagedResult()
  {
    var response = await Client.GetAsync("/api/medidores?page=1&pageSize=10");
    response.EnsureSuccessStatusCode();

    var content = await response.Content.ReadAsStringAsync();
    // Verifica estrutura de paginação
    Assert.Contains("page", content);
    Assert.Contains("pageSize", content);
    Assert.Contains("totalRecords", content);
    Assert.Contains("items", content);
  }

  /// <summary>
  /// Teste 3: POST cadastra medidor e retorna 201 Created.
  /// </summary>
  [Fact]
  public async Task Post_ValidMedidor_ReturnsCreated()
  {
    var response = await Client.PostAsJsonAsync("/api/medidores", new
    {
      nome = "Medidor Novo Teste",
      serial = "HGNEW" + Guid.NewGuid().ToString("N").Substring(0, 8),
      localizacao = "Bloco C"
    });

    Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
  }
}
