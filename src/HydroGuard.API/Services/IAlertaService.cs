using HydroGuard.API.ViewModels;

namespace HydroGuard.API.Services;

/// <summary>
/// Interface do serviço de detecção de vazamentos.
/// </summary>
public interface IAlertaService
{
  Task<List<VazamentoAlertaViewModel>> DetectarVazamentosAsync();
}
