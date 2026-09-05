namespace HydroGuard.API.ViewModels;

/// <summary>
/// ViewModel para retorno de alertas de vazamento.
/// Usado no GET /api/alertas/vazamentos.
/// </summary>
public class VazamentoAlertaViewModel
{
  // ID do medidor com possível vazamento
  public int MedidorId { get; set; }

  // True se vazamento foi detectado
  public bool VazamentoDetectado { get; set; }

  // Severidade: Baixa, Média ou Alta
  public string Severidade { get; set; } = string.Empty;

  // Mensagem descritiva do alerta
  public string Mensagem { get; set; } = string.Empty;
}
