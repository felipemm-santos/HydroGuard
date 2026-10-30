using HydroGuard.API.Models;
using HydroGuard.API.Repositories;
using HydroGuard.API.ViewModels;

namespace HydroGuard.API.Services;

/// <summary>
/// Serviço de Consumos - registra e lista consumo de água.
/// </summary>
public class ConsumoService : IConsumoService
{
  private readonly IConsumoRepository _consumoRepository;
  private readonly IMedidorRepository _medidorRepository;

  public ConsumoService(IConsumoRepository consumoRepository, IMedidorRepository medidorRepository)
  {
    _consumoRepository = consumoRepository;
    _medidorRepository = medidorRepository;
  }

  /// <summary>
  /// Lista consumos com paginação e filtros.
  /// </summary>
  public async Task<PagedResultViewModel<ConsumoViewModel>> GetPagedAsync(
      int page, int pageSize , int? medidorId, DateTime? dataInicial, DateTime? dataFinal)
  {
    page = page < 1 ? 1 : page;
    pageSize = pageSize < 1 ? 10 : pageSize;

    var result = await _consumoRepository.GetPagedAsync(page, pageSize, medidorId, dataInicial, dataFinal);

    return new PagedResultViewModel<ConsumoViewModel>
    {
      Page = result.Page,
      PageSize = result.PageSize,
      TotalRecords = result.TotalRecords,
      Items = result.Items.Select(c => new ConsumoViewModel
      {
        Id = c.Id,
        MedidorId = c.MedidorId,
        LitrosConsumidos = c.LitrosConsumidos,
        DataRegistro = c.DataRegistro
      }).ToList()
    };
  }

  /// <summary>
  /// Registra consumo com data automática e valida medidor existente.
  /// </summary>
  public async Task<ConsumoViewModel> CreateAsync(ConsumoCreateViewModel model)
  {
    // Verifica se o medidor existe
    var medidor = await _medidorRepository.GetByIdAsync(model.MedidorId);
    if (medidor == null)
    {
      throw new KeyNotFoundException("Medidor não encontrado");
    }

    // Cria registro com data automática (DateTime.Now)
    var consumo = new Consumo
    {
      MedidorId = model.MedidorId,
      LitrosConsumidos = model.LitrosConsumidos,
      DataRegistro = DateTime.UtcNow
    };

    var created = await _consumoRepository.CreateAsync(consumo);

    return new ConsumoViewModel
    {
      Id = created.Id,
      MedidorId = created.MedidorId,
      LitrosConsumidos = created.LitrosConsumidos,
      DataRegistro = created.DataRegistro
    };
  }
}
