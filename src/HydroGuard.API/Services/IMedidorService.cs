using HydroGuard.API.ViewModels;

namespace HydroGuard.API.Services;

/// <summary>
/// Interface do serviço de Medidores - lógica de negócio.
/// </summary>
public interface IMedidorService
{
  Task<PagedResultViewModel<MedidorViewModel>> GetPagedAsync(int page, int pageSize);
  Task<MedidorViewModel> CreateAsync(MedidorCreateViewModel model, int userId);
}
