using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using HydroGuard.API.Data;
using HydroGuard.API.Models;
using BCrypt.Net;

namespace HydroGuard.Tests;

/// <summary>
/// Factory customizada para testes de integração.
/// Usa banco InMemory em vez de SQL Server para testes isolados.
/// </summary>
public class HydroGuardWebApplicationFactory : WebApplicationFactory<Program>
{
  /// <summary>
  /// Configura serviços para usar banco InMemory nos testes.
  /// </summary>
  protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
  {
    // Define ambiente de testes para não rodar migrations no Program.cs
    builder.UseEnvironment("Testing");
    builder.UseSetting("Jwt:Key", "TestOnly-Key-At-Least-32-Bytes-Long-2026");
    var databaseName = "HydroGuardTests-" + Guid.NewGuid().ToString("N");

    builder.ConfigureServices(services =>
    {
      // Remove o DbContext SQL Server registrado
      var descriptor = services.SingleOrDefault(
          d => d.ServiceType == typeof(DbContextOptions<HydroGuardDbContext>));

      if (descriptor != null)
        services.Remove(descriptor);

      // Registra DbContext com InMemory para testes
      services.AddDbContext<HydroGuardDbContext>(options =>
      {
        options.UseInMemoryDatabase(databaseName);
      });

      // Popula dados de teste
      using var sp = services.BuildServiceProvider();
      using var scope = sp.CreateScope();
      var context = scope.ServiceProvider.GetRequiredService<HydroGuardDbContext>();
      context.Database.EnsureCreated();
      SeedTestData(context);
    });
  }

  /// <summary>
  /// Popula dados mínimos para os testes funcionarem.
  /// </summary>
  private static void SeedTestData(HydroGuardDbContext context)
  {
    if (context.Users.Any()) return;

    var admin = new User
    {
      Username = "admin",
      PasswordHash = BCrypt.Net.BCrypt.HashPassword("admin123"),
      Role = "Admin"
    };
    context.Users.Add(admin);
    context.SaveChanges();

    var medidor = new Medidor
    {
      Nome = "Medidor Teste",
      Serial = "HGTEST001",
      Localizacao = "Bloco Teste",
      UserId = admin.Id
    };
    context.Medidores.Add(medidor);
    context.SaveChanges();

    // Consumos históricos para testes de vazamento
    for (int i = 5; i >= 1; i--)
    {
      context.Consumos.Add(new Consumo
      {
        MedidorId = medidor.Id,
        LitrosConsumidos = 200 + i * 10,
        DataRegistro = DateTime.UtcNow.AddDays(-i)
      });
    }
    context.SaveChanges();
  }
}

/// <summary>
/// Classe base para testes - fornece client HTTP autenticado.
/// </summary>
public abstract class IntegrationTestBase : IClassFixture<HydroGuardWebApplicationFactory>
{
  protected readonly HttpClient Client;
  protected readonly HydroGuardWebApplicationFactory Factory;

  public IntegrationTestBase(HydroGuardWebApplicationFactory factory)
  {
    Factory = factory;
    Client = factory.CreateClient();
    AuthenticateClient();
  }

  /// <summary>
  /// Faz login e configura token JWT no client HTTP.
  /// </summary>
  protected void AuthenticateClient()
  {
    var loginResponse = Client.PostAsJsonAsync("/api/auth/login", new
    {
      username = "admin",
      password = "admin123"
    }).Result;

    var content = loginResponse.Content.ReadAsStringAsync().Result;
    var json = JsonDocument.Parse(content);
    var token = json.RootElement.GetProperty("token").GetString();

    Client.DefaultRequestHeaders.Authorization =
        new AuthenticationHeaderValue("Bearer", token);
  }
}
