using HydroGuard.API.Models;
using HydroGuard.API.Repositories;
using HydroGuard.API.ViewModels;
using Microsoft.EntityFrameworkCore;
using HydroGuard.API.Data;

namespace HydroGuard.API.Services;

/// <summary>
/// Serviço de alertas - detecta vazamentos comparando consumo atual com média histórica.
/// Regra: consumo atual > média histórica + 40% = possível vazamento.
/// </summary>
public class AlertaService : IAlertaService
{
  private readonly HydroGuardDbContext _context;
  private readonly IConsumoRepository _consumoRepository;
  private readonly IAlertaRepository _alertaRepository;

  public AlertaService(
      HydroGuardDbContext context,
      IConsumoRepository consumoRepository,
      IAlertaRepository alertaRepository)
  {
    _context = context;
    _consumoRepository = consumoRepository;
    _alertaRepository = alertaRepository;
  }

  /// <summary>
  /// Analisa todos os medidores e detecta possíveis vazamentos.
  /// Usa média histórica real dos consumos de cada medidor.
  /// </summary>
  public async Task<List<VazamentoAlertaViewModel>> DetectarVazamentosAsync()
  {
    var alertas = new List<VazamentoAlertaViewModel>();

    // Busca todos os medidores do sistema
    var medidores = await _context.Medidores.ToListAsync();

    foreach (var medidor in medidores)
    {
      // Busca todos os consumos deste medidor
      var consumos = await _consumoRepository.GetByMedidorIdAsync(medidor.Id);

      // Precisa de pelo menos 2 registros para calcular média
      if (consumos.Count < 2)
        continue;

      // Consumo mais recente (último registro)
      var consumoAtual = consumos.First().LitrosConsumidos;

      // Calcula média histórica (excluindo o consumo atual)
      var consumosHistoricos = consumos.Skip(1).ToList();
      var mediaHistorica = consumosHistoricos.Average(c => c.LitrosConsumidos);

      // Limite de alerta: média + 40%
      var limiteAlerta = mediaHistorica * 1.4;

      // Se consumo atual ultrapassa o limite, detecta vazamento
      if (consumoAtual > limiteAlerta)
      {
        // Calcula severidade baseada no percentual acima da média
        var percentualAcima = (consumoAtual - mediaHistorica) / mediaHistorica * 100;
        var severidade = percentualAcima > 80 ? "Alta" : percentualAcima > 50 ? "Média" : "Baixa";

        var alertaVm = new VazamentoAlertaViewModel
        {
          MedidorId = medidor.Id,
          VazamentoDetectado = true,
          Severidade = severidade,
          Mensagem = "Possível vazamento detectado"
        };

        alertas.Add(alertaVm);

        // Salva alerta no banco para histórico
        var alerta = new Alerta
        {
          MedidorId = medidor.Id,
          VazamentoDetectado = true,
          Severidade = severidade,
          Mensagem = "Possível vazamento detectado",
          DataAlerta = DateTime.UtcNow
        };
        await _alertaRepository.CreateAsync(alerta);
      }
    }

    return alertas;
  }
}
