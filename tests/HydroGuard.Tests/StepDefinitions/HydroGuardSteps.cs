using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using Reqnroll;

namespace HydroGuard.Tests.StepDefinitions;

[Binding]
public sealed class HydroGuardSteps : IDisposable
{
  private readonly HydroGuardWebApplicationFactory factory = new();
  private readonly HttpClient client;
  private HttpResponseMessage? response;
  private int medidorId;
  private int litrosEnviados;

  public HydroGuardSteps() => client = factory.CreateClient();

  [Given("que estou autenticado e tenho um medidor cadastrado")]
  public async Task CadastrarMedidor()
  {
    using var login = await client.PostAsJsonAsync("/api/auth/login",
        new { username = "admin", password = "admin123" });
    var token = await ContractAssert.ReadAsync(login, HttpStatusCode.OK, "login");
    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token["token"]!.GetValue<string>());

    using var cadastro = await client.PostAsJsonAsync("/api/medidores",
        new { nome = "Medidor BDD", serial = "HG-BDD", localizacao = "Bloco BDD" });
    var medidor = await ContractAssert.ReadAsync(cadastro, HttpStatusCode.Created, "medidor");
    medidorId = medidor["id"]!.GetValue<int>();
  }

  [Given("que o medidor tem uma leitura histórica de {int} litros")]
  public async Task RegistrarHistorico(int litros)
  {
    using var leitura = await EnviarLeitura(litros);
    await ContractAssert.ReadAsync(leitura, HttpStatusCode.Created, "consumo");
  }

  [When("registro uma leitura de {int} litros")]
  public async Task RegistrarLeitura(int litros)
  {
    litrosEnviados = litros;
    response = await EnviarLeitura(litros);
  }

  [When("registro uma leitura de {int} litros e consulto os alertas")]
  public async Task RegistrarEConsultarAlertas(int litros)
  {
    using var leitura = await EnviarLeitura(litros);
    await ContractAssert.ReadAsync(leitura, HttpStatusCode.Created, "consumo");
    response = await client.GetAsync("/api/alertas/vazamentos");
  }

  [Then("a leitura é criada com os dados enviados e aparece no histórico")]
  public async Task ConferirLeitura()
  {
    var leitura = await ContractAssert.ReadAsync(response!, HttpStatusCode.Created, "consumo");
    Assert.Equal(medidorId, leitura["medidorId"]!.GetValue<int>());
    Assert.Equal((double)litrosEnviados, leitura["litrosConsumidos"]!.GetValue<double>());
    var item = Assert.Single(await LerHistorico())!;
    Assert.Equal(leitura["id"]!.GetValue<int>(), item["id"]!.GetValue<int>());
    Assert.Equal(medidorId, item["medidorId"]!.GetValue<int>());
    Assert.Equal((double)litrosEnviados, item["litrosConsumidos"]!.GetValue<double>());
  }

  [Then("a API rejeita a leitura e o histórico permanece vazio")]
  public async Task ConferirRejeicao()
  {
    await ContractAssert.ReadAsync(response!, HttpStatusCode.BadRequest, "validacao-consumo");
    Assert.Empty(await LerHistorico());
  }

  [Then("recebo um alerta de severidade Alta para esse medidor")]
  public async Task ConferirAlerta()
  {
    var alertas = await ContractAssert.ReadAsync(response!, HttpStatusCode.OK, "vazamentos");
    var alerta = Assert.Single(alertas.AsArray(), item => item!["medidorId"]!.GetValue<int>() == medidorId)!;
    Assert.True(alerta["vazamentoDetectado"]!.GetValue<bool>());
    Assert.Equal("Alta", alerta["severidade"]!.GetValue<string>());
  }

  [Given("que existe um usuário de teste")]
  public async Task ConfirmarUsuario()
  {
    using var login = await client.PostAsJsonAsync("/api/auth/login",
        new { username = "admin", password = "admin123" });
    await ContractAssert.ReadAsync(login, HttpStatusCode.OK, "login");
  }

  [When("tento entrar com uma senha incorreta")]
  public async Task EntrarComSenhaIncorreta()
  {
    response = await client.PostAsJsonAsync("/api/auth/login",
        new { username = "admin", password = "senha-incorreta" });
  }

  [Then("a API rejeita as credenciais sem emitir token")]
  public async Task ConferirCredenciaisInvalidas()
  {
    var erro = await ContractAssert.ReadAsync(response!, HttpStatusCode.Unauthorized, "credenciais-invalidas");
    Assert.Equal("Credenciais inválidas", erro["message"]!.GetValue<string>());
    Assert.False(erro.AsObject().ContainsKey("token"));
  }

  private Task<HttpResponseMessage> EnviarLeitura(int litros) =>
      client.PostAsJsonAsync("/api/consumos", new { medidorId, litrosConsumidos = litros });

  private async Task<JsonArray> LerHistorico()
  {
    using var historico = await client.GetAsync($"/api/consumos?medidorId={medidorId}");
    var pagina = await ContractAssert.ReadAsync(historico, HttpStatusCode.OK, "consumos");
    return pagina["items"]!.AsArray();
  }

  public void Dispose()
  {
    response?.Dispose();
    client.Dispose();
    factory.Dispose();
  }
}
