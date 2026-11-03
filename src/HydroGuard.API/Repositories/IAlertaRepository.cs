using HydroGuard.API.Models;

namespace HydroGuard.API.Repositories;

/// <summary>
/// Interface do repositório de Alertas.
/// </summary>
public interface IAlertaRepository
{
  // Salva um novo alerta no banco
  Task<Alerta> CreateAsync(Alerta alerta);

  // Busca alertas críticos (severidade Alta)
  Task<List<Alerta>> GetCriticosAsync();
}
