using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HydroGuard.API.Services;
using HydroGuard.API.ViewModels;

namespace HydroGuard.API.Controllers;

/// <summary>
/// Controller de Consumos - registro e histórico de consumo de água.
/// POST /api/consumos e GET /api/consumos
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConsumosController : ControllerBase
{
  private readonly IConsumoService _consumoService;

  public ConsumosController(IConsumoService consumoService)
  {
    _consumoService = consumoService;
  }

  /// <summary>
  /// Lista histórico de consumos com paginação e filtros opcionais.
  /// GET /api/consumos?page=1&pageSize=10&medidorId=1&dataInicial=2026-01-01&dataFinal=2026-12-31
  /// </summary>
  [HttpGet]
  public async Task<ActionResult<PagedResultViewModel<ConsumoViewModel>>> Get(
      [FromQuery] int page = 1,
      [FromQuery] int pageSize = 10,
      [FromQuery] int? medidorId = null,
      [FromQuery] DateTime? dataInicial = null,
      [FromQuery] DateTime? dataFinal = null)
  {
    var result = await _consumoService.GetPagedAsync(page, pageSize, medidorId, dataInicial, dataFinal);
    return Ok(result);
  }

  /// <summary>
  /// Registra um novo consumo de água (data salva automaticamente).
  /// POST /api/consumos
  /// </summary>
  [HttpPost]
  public async Task<ActionResult<ConsumoViewModel>> Post([FromBody] ConsumoCreateViewModel model)
  {
    if (!ModelState.IsValid)
    {
      return BadRequest(new ErrorViewModel { Message = "Dados de consumo inválidos" });
    }

    var result = await _consumoService.CreateAsync(model);
    return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
  }
}
