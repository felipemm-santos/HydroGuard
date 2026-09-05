using Microsoft.EntityFrameworkCore;
using HydroGuard.API.Data;
using HydroGuard.API.Repositories;
using HydroGuard.API.ViewModels;

namespace HydroGuard.API.Services;

/// <summary>
/// Serviço de relatórios - agregações LINQ otimizadas no banco.
/// </summary>
public class RelatorioService : IRelatorioService
{
  private readonly HydroGuardDbContext _context;
  private readonly IAlertaRepository _alertaRepository;

  public RelatorioService(HydroGuardDbContext context, IAlertaRepository alertaRepository)
  {
    _context = context;
    _alertaRepository = alertaRepository;
  }

  /// <summary>
  /// Gera relatório gerencial com top medidores, média, alertas críticos e projeção.
  /// </summary>
  public async Task<RelatorioConsumoViewModel> GetRelatorioConsumoAsync()
  {
    var hoje = DateTime.UtcNow;
    var inicioMes = new DateTime(hoje.Year, hoje.Month, 1, 0, 0, 0, DateTimeKind.Utc);
    var fimMes = inicioMes.AddMonths(1).AddTicks(-1);

    // Top 5 medidores por consumo total (agregação LINQ no banco)
    var topMedidores = await _context.Consumos
        .Where(c => c.DataRegistro >= inicioMes && c.DataRegistro <= fimMes)
        .GroupBy(c => new { c.MedidorId, c.Medidor.Nome })
        .Select(g => new TopMedidorViewModel
        {
          MedidorId = g.Key.MedidorId,
          Nome = g.Key.Nome,
          TotalConsumo = g.Sum(c => c.LitrosConsumidos)
        })
        .OrderByDescending(t => t.TotalConsumo)
        .Take(5)
        .ToListAsync();

    // Média de consumo por período (mês atual)
    var mediaPorPeriodo = await _context.Consumos
        .Where(c => c.DataRegistro >= inicioMes && c.DataRegistro <= fimMes)
        .AverageAsync(c => (double?)c.LitrosConsumidos) ?? 0;

    // Alertas críticos (severidade Alta)
    var alertasCriticos = await _alertaRepository.GetCriticosAsync();
    var alertasVm = alertasCriticos.Select(a => new VazamentoAlertaViewModel
    {
      MedidorId = a.MedidorId,
      VazamentoDetectado = a.VazamentoDetectado,
      Severidade = a.Severidade,
      Mensagem = a.Mensagem
    }).ToList();

    // Projeção futura: média diária * dias do próximo mês
    var diasProximoMes = DateTime.DaysInMonth(
        hoje.Month == 12 ? hoje.Year + 1 : hoje.Year,
        hoje.Month == 12 ? 1 : hoje.Month + 1);

    var consumoTotalMes = topMedidores.Sum(t => t.TotalConsumo);
    var diasNoMes = DateTime.DaysInMonth(hoje.Year, hoje.Month);
    var mediaDiaria = diasNoMes > 0 ? consumoTotalMes / diasNoMes : 0;
    var projecaoFutura = mediaDiaria * diasProximoMes;

    return new RelatorioConsumoViewModel
    {
      TopMedidores = topMedidores,
      MediaPorPeriodo = Math.Round(mediaPorPeriodo, 2),
      AlertasCriticos = alertasVm,
      ProjecaoFutura = Math.Round(projecaoFutura, 2)
    };
  }
}
