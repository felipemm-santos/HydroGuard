using HydroGuard.API.Models;
using HydroGuard.API.Repositories;
using HydroGuard.API.ViewModels;

namespace HydroGuard.API.Services;

/// <summary>
/// Serviço de Medidores - implementa a lógica de negócio.
/// Valida dados e chama o repositório para persistência.
/// </summary>
public class MedidorService : IMedidorService
{
  private readonly IMedidorRepository _medidorRepository;

  public MedidorService(IMedidorRepository medidorRepository)
  {
    _medidorRepository = medidorRepository;
  }

  /// <summary>
  /// Lista medidores paginados convertendo Model para ViewModel.
  /// </summary>
  public async Task<PagedResultViewModel<MedidorViewModel>> GetPagedAsync(int page, int pageSize)
  {
    // Garante valores válidos de paginação
    page = page < 1 ? 1 : page;
    pageSize = pageSize < 1 ? 10 : pageSize;

    // Busca no repositório
    var result = await _medidorRepository.GetPagedAsync(page, pageSize);

    // Converte Models para ViewModels (não expor entidades diretamente)
    return new PagedResultViewModel<MedidorViewModel>
    {
      Page = result.Page,
      PageSize = result.PageSize,
      TotalRecords = result.TotalRecords,
      Items = result.Items.Select(m => new MedidorViewModel
      {
        Id = m.Id,
        Nome = m.Nome,
        Serial = m.Serial,
        Localizacao = m.Localizacao
      }).ToList()
    };
  }

  /// <summary>
  /// Cadastra um novo medidor validando serial único.
  /// </summary>
  public async Task<MedidorViewModel> CreateAsync(MedidorCreateViewModel model, int userId)
  {
    // Valida se o serial já existe (deve ser único)
    if (await _medidorRepository.SerialExistsAsync(model.Serial))
    {
      throw new InvalidOperationException("O serial do medidor já está cadastrado");
    }

    // Cria a entidade Medidor a partir do ViewModel
    var medidor = new Medidor
    {
      Nome = model.Nome,
      Serial = model.Serial,
      Localizacao = model.Localizacao,
      UserId = userId
    };

    // Salva no banco via repositório
    var created = await _medidorRepository.CreateAsync(medidor);

    // Retorna ViewModel com os dados criados
    return new MedidorViewModel
    {
      Id = created.Id,
      Nome = created.Nome,
      Serial = created.Serial,
      Localizacao = created.Localizacao
    };
  }
}
