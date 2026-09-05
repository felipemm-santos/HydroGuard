namespace HydroGuard.API.Models;

/// <summary>
/// Modelo que representa um alerta de vazamento ou consumo anormal.
/// Gerado automaticamente quando o consumo ultrapassa a média histórica + 40%.
/// </summary>
public class Alerta
{
  // Identificador único do alerta (chave primária)
  public int Id { get; set; }

  // Chave estrangeira: ID do medidor com possível vazamento
  public int MedidorId { get; set; }

  // Indica se foi detectado vazamento (true = possível vazamento)
  public bool VazamentoDetectado { get; set; }

  // Nível de severidade: Baixa, Média ou Alta
  public string Severidade { get; set; } = "Média";

  // Mensagem descritiva do alerta
  public string Mensagem { get; set; } = string.Empty;

  // Data em que o alerta foi gerado
  public DateTime DataAlerta { get; set; }

  // Relacionamento com o medidor
  public Medidor Medidor { get; set; } = null!;
}
