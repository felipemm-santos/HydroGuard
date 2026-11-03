using HydroGuard.API.ViewModels;

namespace HydroGuard.API.Services;

/// <summary>
/// Interface do serviço de Consumos.
/// </summary>
public interface IConsumoService
{
  Task<PagedResultViewModel<ConsumoViewModel>> GetPagedAsync(
      int page, int pageSize, int? medidorId, DateTime? dataInicial, DateTime? dataFinal);

  Task<ConsumoViewModel> CreateAsync(ConsumoCreateViewModel model);
}
