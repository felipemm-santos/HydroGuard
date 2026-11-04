using System.Net.Http.Headers;

namespace HydroGuard.Tests.Controllers;

public class ApiContractTests : IntegrationTestBase
{
  public ApiContractTests(HydroGuardWebApplicationFactory factory) : base(factory) { }

  [Fact]
  public async Task Login_ContratoRetornaTokenAceitoPelaApi()
  {
    using var client = Factory.CreateClient();
    using var response = await client.PostAsJsonAsync("/api/auth/login",
        new { username = "admin", password = "admin123" });
    var body = await ContractAssert.ReadAsync(response, HttpStatusCode.OK, "login");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", body["token"]!.GetValue<string>());
    using var protegida = await client.GetAsync("/api/medidores");
    Assert.Equal(HttpStatusCode.OK, protegida.StatusCode);
  }

  [Fact]
  public async Task ListarMedidores_ContratoRetornaPaginaComMedidorEsperado()
  {
    using var response = await Client.GetAsync("/api/medidores?page=1&pageSize=1");
    var body = await ContractAssert.ReadAsync(response, HttpStatusCode.OK, "medidores");
    Assert.Equal(1, body["page"]!.GetValue<int>());
    Assert.Equal(1, body["pageSize"]!.GetValue<int>());
    var item = Assert.Single(body["items"]!.AsArray())!;
    Assert.Equal("HGTEST001", item["serial"]!.GetValue<string>());
  }

  [Fact]
  public async Task CriarMedidor_ContratoRetornaDadosDoCadastro()
  {
    var cadastro = new { nome = "Medidor Contrato", serial = "HG-CONTRATO", localizacao = "Bloco C" };
    using var response = await Client.PostAsJsonAsync("/api/medidores", cadastro);
    var body = await ContractAssert.ReadAsync(response, HttpStatusCode.Created, "medidor");
    Assert.Equal(cadastro.nome, body["nome"]!.GetValue<string>());
    Assert.Equal(cadastro.serial, body["serial"]!.GetValue<string>());
    Assert.Equal(cadastro.localizacao, body["localizacao"]!.GetValue<string>());
  }

  [Fact]
  public async Task Indicadores_ContratoRetornaConsumoDoMes()
  {
    using var cadastro = await Client.PostAsJsonAsync("/api/consumos",
        new { medidorId = 1, litrosConsumidos = 500 });
    Assert.Equal(HttpStatusCode.Created, cadastro.StatusCode);

    using var response = await Client.GetAsync("/api/esg/indicadores");
    var body = await ContractAssert.ReadAsync(response, HttpStatusCode.OK, "esg");
    Assert.True(body["consumoMensal"]!.GetValue<double>() >= 500);
    Assert.InRange(body["scoreESG"]!.GetValue<int>(), 0, 100);
  }

  [Fact]
  public async Task Relatorio_ContratoRetornaMedidorNoRanking()
  {
    using var cadastro = await Client.PostAsJsonAsync("/api/consumos",
        new { medidorId = 1, litrosConsumidos = 500 });
    Assert.Equal(HttpStatusCode.Created, cadastro.StatusCode);

    using var response = await Client.GetAsync("/api/relatorios/consumo");
    var body = await ContractAssert.ReadAsync(response, HttpStatusCode.OK, "relatorio");
    var item = Assert.Single(body["topMedidores"]!.AsArray())!;
    Assert.Equal(1, item["medidorId"]!.GetValue<int>());
    Assert.Equal("Medidor Teste", item["nome"]!.GetValue<string>());
    Assert.True(item["totalConsumo"]!.GetValue<double>() >= 500);
  }
}
