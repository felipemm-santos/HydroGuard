namespace HydroGuard.Tests.Controllers;

/// <summary>
/// Testes unitários do AuthController.
/// Valida login e autenticação JWT.
/// </summary>
public class AuthControllerTests : IntegrationTestBase
{
  public AuthControllerTests(HydroGuardWebApplicationFactory factory) : base(factory) { }

  /// <summary>
  /// Teste 1: Login com credenciais válidas retorna status 200.
  /// </summary>
  [Fact]
  public async Task Login_ValidCredentials_ReturnsHttpStatusCode200()
  {
    // Cria client sem autenticação para testar login
    var client = Factory.CreateClient();

    var response = await client.PostAsJsonAsync("/api/auth/login", new
    {
      username = "admin",
      password = "admin123"
    });

    // Verifica que retornou 200 OK
    response.EnsureSuccessStatusCode();
    Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
  }

  /// <summary>
  /// Teste 2: Login retorna token JWT no corpo da resposta.
  /// </summary>
  [Fact]
  public async Task Login_ValidCredentials_ReturnsToken()
  {
    var client = Factory.CreateClient();

    var response = await client.PostAsJsonAsync("/api/auth/login", new
    {
      username = "admin",
      password = "admin123"
    });

    response.EnsureSuccessStatusCode();

    var content = await response.Content.ReadAsStringAsync();
    // Verifica que o token está presente na resposta
    Assert.Contains("token", content);
  }

  /// <summary>
  /// Teste 3: Login com credenciais inválidas retorna 401.
  /// </summary>
  [Fact]
  public async Task Login_InvalidCredentials_ReturnsUnauthorized()
  {
    var client = Factory.CreateClient();

    var response = await client.PostAsJsonAsync("/api/auth/login", new
    {
      username = "admin",
      password = "senhaerrada"
    });

    Assert.Equal(System.Net.HttpStatusCode.Unauthorized, response.StatusCode);
  }
}
