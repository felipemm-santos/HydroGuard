namespace HydroGuard.API.Models;

/// <summary>
/// Modelo que representa um registro de consumo de água.
/// Cada consumo está vinculado a um medidor específico.
/// </summary>
public class Consumo
{
  // Identificador único do registro de consumo (chave primária)
  public int Id { get; set; }

  // Chave estrangeira: ID do medidor que registrou o consumo
  public int MedidorId { get; set; }

  // Quantidade de litros consumidos (não pode ser negativo)
  public double LitrosConsumidos { get; set; }

  // Data e hora do registro (salva automaticamente no POST)
  public DateTime DataRegistro { get; set; }

  // Relacionamento com o medidor
  public Medidor Medidor { get; set; } = null!;
}
