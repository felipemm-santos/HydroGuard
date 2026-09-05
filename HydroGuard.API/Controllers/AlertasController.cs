using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using HydroGuard.API.Services;
using HydroGuard.API.ViewModels;

namespace HydroGuard.API.Controllers;

/// <summary>
/// Controller de Alertas - detecção de vazamentos.
/// GET /api/alertas/vazamentos
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AlertasController : ControllerBase
{
  private readonly IAlertaService _alertaService;

  public AlertasController(IAlertaService alertaService)
  {
    _alertaService = alertaService;
  }

  /// <summary>
  /// Detecta possíveis vazamentos comparando consumo atual com média histórica + 40%.
  /// GET /api/alertas/vazamentos
  /// </summary>
  [HttpGet("vazamentos")]
  public async Task<ActionResult<List<VazamentoAlertaViewModel>>> GetVazamentos()
  {
    var alertas = await _alertaService.DetectarVazamentosAsync();
    return Ok(alertas);
  }
}
