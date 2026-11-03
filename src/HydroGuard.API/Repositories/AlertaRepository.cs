using Microsoft.EntityFrameworkCore;
using HydroGuard.API.Data;
using HydroGuard.API.Models;

namespace HydroGuard.API.Repositories;

/// <summary>
/// Implementação do repositório de Alertas.
/// </summary>
public class AlertaRepository : IAlertaRepository
{
  private readonly HydroGuardDbContext _context;

  public AlertaRepository(HydroGuardDbContext context)
  {
    _context = context;
  }

  /// <summary>
  /// Salva um alerta de vazamento no banco.
  /// </summary>
  public async Task<Alerta> CreateAsync(Alerta alerta)
  {
    _context.Alertas.Add(alerta);
    await _context.SaveChangesAsync();
    return alerta;
  }

  /// <summary>
  /// Busca alertas com severidade Alta (críticos).
  /// </summary>
  public async Task<List<Alerta>> GetCriticosAsync()
  {
    return await _context.Alertas
        .Where(a => a.Severidade == "Alta")
        .OrderByDescending(a => a.DataAlerta)
        .ToListAsync();
  }
}
