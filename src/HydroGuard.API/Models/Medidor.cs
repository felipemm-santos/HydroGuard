namespace HydroGuard.API.Models;

/// <summary>
/// Modelo que representa um hidrômetro/medidor de água.
/// Cada medidor pertence a um usuário e registra consumos e alertas.
/// </summary>
public class Medidor
{
  // Identificador único do medidor (chave primária)
  public int Id { get; set; }

  // Nome descritivo do medidor (ex: "Medidor Cozinha")
  public string Nome { get; set; } = string.Empty;

  // Número serial único do hidrômetro (ex: "HG123456")
  public string Serial { get; set; } = string.Empty;

  // Localização física do medidor (ex: "Bloco A")
  public string Localizacao { get; set; } = string.Empty;

  // Chave estrangeira: ID do usuário que cadastrou o medidor
  public int UserId { get; set; }

  // Relacionamento com o usuário dono do medidor
  public User User { get; set; } = null!;

  // Relacionamento: um medidor tem vários registros de consumo
  public List<Consumo> Consumos { get; set; } = new();

  // Relacionamento: um medidor pode ter vários alertas de vazamento
  public List<Alerta> Alertas { get; set; } = new();
}
