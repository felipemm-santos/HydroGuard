using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HydroGuard.API.Services;
using HydroGuard.API.ViewModels;

namespace HydroGuard.API.Controllers;

/// <summary>
/// Controller ESG - dashboard com indicadores de sustentabilidade.
/// GET /api/esg/indicadores
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EsgController : ControllerBase
{
  private readonly IEsgService _esgService;

  public EsgController(IEsgService esgService)
  {
    _esgService = esgService;
  }

  /// <summary>
  /// Retorna indicadores ESG: consumo mensal, média diária, desperdício, economia e score.
  /// GET /api/esg/indicadores
  /// </summary>
  [HttpGet("indicadores")]
  public async Task<ActionResult<EsgIndicadoresViewModel>> GetIndicadores()
  {
    var indicadores = await _esgService.GetIndicadoresAsync();
    return Ok(indicadores);
  }
}
