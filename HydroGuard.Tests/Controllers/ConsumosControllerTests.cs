namespace HydroGuard.Tests.Controllers;

/// <summary>
/// Testes unitários do ConsumosController.
/// Valida registro e histórico de consumo.
/// </summary>
public class ConsumosControllerTests : IntegrationTestBase
{
  public ConsumosControllerTests(HydroGuardWebApplicationFactory factory) : base(factory) { }

  /// <summary>
  /// Teste 1: GET /api/consumos retorna status 200.
  /// </summary>
  [Fact]
  public async Task Get_ReturnsHttpStatusCode200()
  {
    var response = await Client.GetAsync("/api/consumos?page=1&pageSize=10");
    response.EnsureSuccessStatusCode();
    Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
  }

  /// <summary>
  /// Teste 2: GET com filtro por medidorId retorna 200.
  /// </summary>
  [Fact]
  public async Task Get_WithMedidorIdFilter_Returns200()
  {
    var response = await Client.GetAsync("/api/consumos?page=1&pageSize=10&medidorId=1");
    response.EnsureSuccessStatusCode();
    Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
  }

  /// <summary>
  /// Teste 3: POST registra consumo e retorna 201 Created.
  /// </summary>
  [Fact]
  public async Task Post_ValidConsumo_ReturnsCreated()
  {
    var response = await Client.PostAsJsonAsync("/api/consumos", new
    {
      medidorId = 1,
      litrosConsumidos = 350.5
    });

    Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
  }
}
