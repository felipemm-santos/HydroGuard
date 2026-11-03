using Microsoft.EntityFrameworkCore;
using HydroGuard.API.Data;
using HydroGuard.API.Models;
using HydroGuard.API.ViewModels;

namespace HydroGuard.API.Repositories;

/// <summary>
/// Implementação do repositório de Consumos.
/// </summary>
public class ConsumoRepository : IConsumoRepository
{
  private readonly HydroGuardDbContext _context;

  public ConsumoRepository(HydroGuardDbContext context)
  {
    _context = context;
  }

  /// <summary>
  /// Lista consumos com paginação e filtros opcionais por medidor e data.
  /// </summary>
  public async Task<PagedResultViewModel<Consumo>> GetPagedAsync(
      int page, int pageSize, int? medidorId, DateTime? dataInicial, DateTime? dataFinal)
  {
    // Query base de consumos
    var query = _context.Consumos.AsQueryable();

    // Filtro opcional por medidor
    if (medidorId.HasValue)
    {
      query = query.Where(c => c.MedidorId == medidorId.Value);
    }

    // Filtro opcional por data inicial
    if (dataInicial.HasValue)
    {
      query = query.Where(c => c.DataRegistro >= dataInicial.Value);
    }

    // Filtro opcional por data final
    if (dataFinal.HasValue)
    {
      query = query.Where(c => c.DataRegistro <= dataFinal.Value);
    }

    // Total de registros após filtros
    var totalRecords = await query.CountAsync();

    // Busca paginada ordenada por data (mais recente primeiro)
    var items = await query
        .OrderByDescending(c => c.DataRegistro)
        .Skip((page - 1) * pageSize)
        .Take(pageSize)
        .ToListAsync();

    return new PagedResultViewModel<Consumo>
    {
      Page = page,
      PageSize = pageSize,
      TotalRecords = totalRecords,
      Items = items
    };
  }

  /// <summary>
  /// Registra um novo consumo no banco.
  /// </summary>
  public async Task<Consumo> CreateAsync(Consumo consumo)
  {
    _context.Consumos.Add(consumo);
    await _context.SaveChangesAsync();
    return consumo;
  }

  /// <summary>
  /// Busca todos os consumos de um medidor (para análise de vazamento).
  /// </summary>
  public async Task<List<Consumo>> GetByMedidorIdAsync(int medidorId)
  {
    return await _context.Consumos
        .Where(c => c.MedidorId == medidorId)
        .OrderByDescending(c => c.DataRegistro)
        .ToListAsync();
  }

  /// <summary>
  /// Busca consumos em um intervalo de datas.
  /// </summary>
  public async Task<List<Consumo>> GetByPeriodoAsync(DateTime inicio, DateTime fim)
  {
    return await _context.Consumos
        .Where(c => c.DataRegistro >= inicio && c.DataRegistro <= fim)
        .ToListAsync();
  }
}
