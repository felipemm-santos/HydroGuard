using HydroGuard.API.Repositories;
using HydroGuard.API.ViewModels;

namespace HydroGuard.API.Services;

/// <summary>
/// Serviço ESG - calcula indicadores de sustentabilidade para o dashboard.
/// </summary>
public class EsgService : IEsgService
{
  private readonly IConsumoRepository _consumoRepository;

  public EsgService(IConsumoRepository consumoRepository)
  {
    _consumoRepository = consumoRepository;
  }

  /// <summary>
  /// Calcula indicadores ESG: consumo mensal, média diária, desperdício, economia e score.
  /// </summary>
  public async Task<EsgIndicadoresViewModel> GetIndicadoresAsync()
  {
    // Define período do mês atual
    var hoje = DateTime.UtcNow;
    var inicioMes = new DateTime(hoje.Year, hoje.Month, 1, 0, 0, 0, DateTimeKind.Utc);
    var fimMes = inicioMes.AddMonths(1).AddTicks(-1);

    // Define período do mês anterior (para comparar economia)
    var inicioMesAnterior = inicioMes.AddMonths(-1);
    var fimMesAnterior = inicioMes.AddTicks(-1);

    // Busca consumos do mês atual
    var consumosMes = await _consumoRepository.GetByPeriodoAsync(inicioMes, fimMes);
    var consumosMesAnterior = await _consumoRepository.GetByPeriodoAsync(inicioMesAnterior, fimMesAnterior);

    // Consumo total mensal em litros
    var consumoMensal = consumosMes.Sum(c => c.LitrosConsumidos);

    // Dias no mês para calcular média diária
    var diasNoMes = DateTime.DaysInMonth(hoje.Year, hoje.Month);
    var consumoMedioDiario = diasNoMes > 0 ? consumoMensal / diasNoMes : 0;

    // Média histórica geral para estimar desperdício
    var mediaGeral = consumosMes.Count > 0 ? consumosMes.Average(c => c.LitrosConsumidos) : 0;
    var desperdicioEstimado = consumosMes
        .Where(c => c.LitrosConsumidos > mediaGeral * 1.4)
        .Sum(c => c.LitrosConsumidos - mediaGeral);

    // Água economizada: diferença entre mês anterior e atual (se economizou)
    var consumoMesAnterior = consumosMesAnterior.Sum(c => c.LitrosConsumidos);
    var aguaEconomizada = consumoMesAnterior > consumoMensal
        ? consumoMesAnterior - consumoMensal
        : 0;

    // Score ESG de 0 a 100 (baseado em desperdício e economia)
    var scoreESG = CalcularScoreESG(consumoMensal, desperdicioEstimado, aguaEconomizada);

    return new EsgIndicadoresViewModel
    {
      ConsumoMensal = Math.Round(consumoMensal, 2),
      ConsumoMedioDiario = Math.Round(consumoMedioDiario, 2),
      DesperdicioEstimado = Math.Round(desperdicioEstimado, 2),
      AguaEconomizada = Math.Round(aguaEconomizada, 2),
      ScoreESG = scoreESG
    };
  }

  /// <summary>
  /// Calcula score ESG de 0 a 100 baseado em indicadores de sustentabilidade.
  /// </summary>
  private static int CalcularScoreESG(double consumoMensal, double desperdicio, double economia)
  {
    // Score base de 70 pontos
    var score = 70;

    // Bônus por economia de água (+1 ponto por cada 100 litros economizados, max 20)
    if (economia > 0)
      score += Math.Min(20, (int)(economia / 100));

    // Penalidade por desperdício (-1 ponto por cada 50 litros desperdiçados, max -30)
    if (desperdicio > 0)
      score -= Math.Min(30, (int)(desperdicio / 50));

    // Garante que score fica entre 0 e 100
    return Math.Clamp(score, 0, 100);
  }
}
