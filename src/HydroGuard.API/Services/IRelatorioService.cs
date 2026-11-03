using HydroGuard.API.ViewModels;

namespace HydroGuard.API.Services;

/// <summary>
/// Interface do serviço de relatórios gerenciais.
/// </summary>
public interface IRelatorioService
{
  Task<RelatorioConsumoViewModel> GetRelatorioConsumoAsync();
}
