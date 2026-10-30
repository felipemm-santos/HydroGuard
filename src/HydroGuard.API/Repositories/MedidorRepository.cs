using Microsoft.EntityFrameworkCore;
using HydroGuard.API.Data;
using HydroGuard.API.Models;
using HydroGuard.API.ViewModels;

namespace HydroGuard.API.Repositories;

/// <summary>
/// Implementação do repositório de Medidores usando Entity Framework Core.
/// Responsável por todas as operações de CRUD na tabela Medidores.
/// </summary>
public class MedidorRepository : IMedidorRepository
{
  // Contexto do banco de dados injetado via DI
  private readonly HydroGuardDbContext _context;

  // Construtor que recebe o DbContext
  public MedidorRepository(HydroGuardDbContext context)
  {
    _context = context;
  }

  /// <summary>
  /// Retorna medidores paginados - usado no GET /api/medidores.
  /// </summary>
  public async Task<PagedResultViewModel<Medidor>> GetPagedAsync(int page, int pageSize)
  {
    // Conta total de registros para a paginação
    var totalRecords = await _context.Medidores.CountAsync();

    // Busca os itens da página atual (Skip/Take para paginação)
    var items = await _context.Medidores
        .OrderBy(m => m.Id)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

  // Retorna o resultado paginado
    return new PagedResultViewModel<Medidor>
    {
      Page = page,
      PageSize = pageSize,
      TotalRecords = totalRecords,
      Items = items
    };
  }

  /// <summary>
  /// Busca um medidor específico pelo ID.
  /// </summary>
  public async Task<Medidor?> GetByIdAsync(int id)
  {
    return await _context.Medidores.FindAsync(id);
  }

  /// <summary>
  /// Verifica se já existe medidor com o serial informado.
  /// Usado para validar unicidade antes do cadastro.
  /// </summary>
  public async Task<bool> SerialExistsAsync(string serial)
  {
    return await _context.Medidores.AnyAsync(m => m.Serial == serial);
  }

  /// <summary>
  /// Insere um novo medidor no banco de dados.
  /// </summary>
  public async Task<Medidor> CreateAsync(Medidor medidor)
  {
    // Adiciona o medidor ao contexto
    _context.Medidores.Add(medidor);

    // Salva as alterações no banco
    await _context.SaveChangesAsync();

    return medidor;
  }
}
