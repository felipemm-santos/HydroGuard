using HydroGuard.API.ViewModels;

namespace HydroGuard.API.Services;

/// <summary>
/// Interface do serviço de indicadores ESG.
/// </summary>
public interface IEsgService
{
  Task<EsgIndicadoresViewModel> GetIndicadoresAsync();
}
