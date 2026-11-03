namespace HydroGuard.API.ViewModels;

/// <summary>
/// ViewModel para indicadores ESG do dashboard.
/// Usado no GET /api/esg/indicadores.
/// </summary>
public class EsgIndicadoresViewModel
{
  // Consumo total do mês atual em litros
  public double ConsumoMensal { get; set; }

  // Consumo médio diário em litros
  public double ConsumoMedioDiario { get; set; }

  // Desperdício estimado (consumo acima da média histórica)
  public double DesperdicioEstimado { get; set; }

  // Água economizada (comparado com mês anterior)
  public double AguaEconomizada { get; set; }

  // Score ESG de 0 a 100 (sustentabilidade)
  public int ScoreESG { get; set; }
}
