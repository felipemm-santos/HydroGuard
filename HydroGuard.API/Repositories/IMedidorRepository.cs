using HydroGuard.API.Models;
using HydroGuard.API.ViewModels;

namespace HydroGuard.API.Repositories;

/// <summary>
/// Interface do repositório de Medidores - define as operações de acesso a dados.
/// Segue o padrão Repository para separar lógica de negócio da persistência.
/// </summary>
public interface IMedidorRepository
{
  // Busca medidores com paginação
  Task<PagedResultViewModel<Medidor>> GetPagedAsync(int page, int pageSize);

  // Busca um medidor pelo ID
  Task<Medidor?> GetByIdAsync(int id);

  // Verifica se o serial já existe (para validação de unicidade)
  Task<bool> SerialExistsAsync(string serial);

  // Cadastra um novo medidor
  Task<Medidor> CreateAsync(Medidor medidor);
}
