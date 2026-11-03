using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HydroGuard.API.Services;
using HydroGuard.API.ViewModels;

namespace HydroGuard.API.Controllers;

/// <summary>
/// Controller de Relatórios - relatório gerencial de consumo.
/// GET /api/relatorios/consumo
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RelatoriosController : ControllerBase
{
  private readonly IRelatorioService _relatorioService;

  public RelatoriosController(IRelatorioService relatorioService)
  {
    _relatorioService = relatorioService;
  }

  /// <summary>
  /// Gera relatório gerencial com top medidores, média, alertas críticos e projeção.
  /// GET /api/relatorios/consumo
  /// </summary>
  [HttpGet("consumo")]
  public async Task<ActionResult<RelatorioConsumoViewModel>> GetConsumo()
  {
    var relatorio = await _relatorioService.GetRelatorioConsumoAsync();
    return Ok(relatorio);
  }
}
