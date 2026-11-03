using Microsoft.EntityFrameworkCore;
using HydroGuard.API.Models;

namespace HydroGuard.API.Data;

/// <summary>
/// Classe para popular o banco com dados iniciais (seed).
/// Cria admin configurado externamente e, opcionalmente, dados demonstrativos.
/// </summary>
public static class DbSeeder
{
  /// <summary>
  /// Inicializa usuários se ainda não existem. Dados demonstrativos são opcionais.
  /// </summary>
  public static async Task SeedAsync(HydroGuardDbContext context, string adminPassword, bool includeDemoData)
  {
    // Só faz seed se não existem usuários
    if (await context.Users.AnyAsync())
      return;

    // Cria admin com senha fornecida externamente; não redefine usuários existentes.
    var admin = new User
    {
      Username = "admin",
      PasswordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword),
      Role = "Admin"
    };

    context.Users.Add(admin);
    await context.SaveChangesAsync();
    if (!includeDemoData) return;

    // Cria medidores de exemplo
    var medidor1 = new Medidor
    {
      Nome = "Medidor Cozinha",
      Serial = "HG123456",
      Localizacao = "Bloco A",
      UserId = admin.Id
    };

    var medidor2 = new Medidor
    {
      Nome = "Medidor Banheiro",
      Serial = "HG789012",
      Localizacao = "Bloco B",
      UserId = admin.Id
    };

    var medidor3 = new Medidor
    {
      Nome = "Medidor Jardim",
      Serial = "HG345678",
      Localizacao = "Área Externa",
      UserId = admin.Id
    };

    context.Medidores.AddRange(medidor1, medidor2, medidor3);
    await context.SaveChangesAsync();

    // Cria consumos de exemplo (histórico para análise de vazamento)
    var consumos = new List<Consumo>();
    var random = new Random(42); // Seed fixo para reprodutibilidade

    foreach (var medidor in new[] { medidor1, medidor2, medidor3 })
    {
      // Gera 10 consumos históricos com valores normais
      for (int i = 10; i >= 1; i--)
      {
        consumos.Add(new Consumo
        {
          MedidorId = medidor.Id,
          LitrosConsumidos = 200 + random.Next(0, 100), // Entre 200-300 litros
          DataRegistro = DateTime.UtcNow.AddDays(-i)
        });
      }
    }

    // Adiciona um consumo alto no medidor 1 para simular vazamento
    consumos.Add(new Consumo
    {
      MedidorId = medidor1.Id,
      LitrosConsumidos = 500, // Bem acima da média (~250)
      DataRegistro = DateTime.UtcNow
    });

    context.Consumos.AddRange(consumos);
    await context.SaveChangesAsync();
  }
}
