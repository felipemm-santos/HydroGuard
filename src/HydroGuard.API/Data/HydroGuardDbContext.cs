using Microsoft.EntityFrameworkCore;
using HydroGuard.API.Models;

namespace HydroGuard.API.Data;

/// <summary>
/// DbContext do Entity Framework - representa o banco de dados HydroGuard.
/// Configura as tabelas, relacionamentos e índices.
/// </summary>
public class HydroGuardDbContext : DbContext
{
  // Construtor que recebe as opções de configuração do banco
  public HydroGuardDbContext(DbContextOptions<HydroGuardDbContext> options)
      : base(options)
  {
  }

  // Tabela de usuários do sistema
  public DbSet<User> Users => Set<User>();

  // Tabela de medidores/hidrômetros
  public DbSet<Medidor> Medidores => Set<Medidor>();

  // Tabela de registros de consumo de água
  public DbSet<Consumo> Consumos => Set<Consumo>();

  // Tabela de alertas de vazamento
  public DbSet<Alerta> Alertas => Set<Alerta>();

  /// <summary>
  /// Configura os relacionamentos e constraints do banco de dados.
  /// Chamado automaticamente pelo EF Core na criação do modelo.
  /// </summary>
  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    // Configuração da tabela Users
    modelBuilder.Entity<User>(entity =>
    {
      // Username deve ser único no banco
      entity.HasIndex(u => u.Username).IsUnique();
    });

    // Configuração da tabela Medidores
    modelBuilder.Entity<Medidor>(entity =>
    {
      // Serial do medidor deve ser único (não pode repetir)
      entity.HasIndex(m => m.Serial).IsUnique();

      // Relacionamento: Medidor pertence a um User (1:N)
      entity.HasOne(m => m.User)
            .WithMany(u => u.Medidores)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    });

    // Configuração da tabela Consumos
    modelBuilder.Entity<Consumo>(entity =>
    {
      // Relacionamento: Consumo pertence a um Medidor (1:N)
      entity.HasOne(c => c.Medidor)
            .WithMany(m => m.Consumos)
            .HasForeignKey(c => c.MedidorId)
            .OnDelete(DeleteBehavior.Cascade);
    });

    // Configuração da tabela Alertas
    modelBuilder.Entity<Alerta>(entity =>
    {
      // Relacionamento: Alerta pertence a um Medidor (1:N)
      entity.HasOne(a => a.Medidor)
            .WithMany(m => m.Alertas)
            .HasForeignKey(a => a.MedidorId)
            .OnDelete(DeleteBehavior.Cascade);
    });
  }
}
