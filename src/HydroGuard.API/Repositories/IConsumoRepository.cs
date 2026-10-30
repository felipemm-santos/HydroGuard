using HydroGuard.API.Models;
using HydroGuard.API.ViewModels;

namespace HydroGuard.API.Repositories;

/// <summary>
/// Interface do repositório de Consumos.
/// </summary>
public interface IConsumoRepository
{
  // Lista consumos com paginação e filtros opcionais
  Task<PagedResultViewModel<Consumo>> GetPagedAsync(
      int page, int pageSize, int? medidorId, DateTime? dataInicial, DateTime? dataFinal);

  // Registra um novo consumo
  Task<Consumo> CreateAsync(Consumo consumo);

  // Busca todos os consumos de um medidor (para cálculo de média)
  Task<List<Consumo>> GetByMedidorIdAsync(int medidorId);

  // Busca consumos por período (para relatórios e ESG)
  Task<List<Consumo>> GetByPeriodoAsync(DateTime inicio, DateTime fim);
}
