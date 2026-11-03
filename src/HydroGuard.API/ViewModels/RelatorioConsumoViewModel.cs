namespace HydroGuard.API.ViewModels;

/// <summary>
/// ViewModel para o relatório gerencial de consumo.
/// Usado no GET /api/relatorios/consumo.
/// </summary>
public class RelatorioConsumoViewModel
{
  // Top medidores com maior consumo
  public List<TopMedidorViewModel> TopMedidores { get; set; } = new();

  // Média de consumo por período
  public double MediaPorPeriodo { get; set; }

  // Alertas críticos (severidade Alta)
  public List<VazamentoAlertaViewModel> AlertasCriticos { get; set; } = new();

  // Projeção de consumo futuro (próximo mês)
  public double ProjecaoFutura { get; set; }
}

/// <summary>
/// ViewModel auxiliar para top medidores no relatório.
/// </summary>
public class TopMedidorViewModel
{
  // ID do medidor
  public int MedidorId { get; set; }

  // Nome do medidor
  public string Nome { get; set; } = string.Empty;

  // Total consumido em litros
  public double TotalConsumo { get; set; }
}
