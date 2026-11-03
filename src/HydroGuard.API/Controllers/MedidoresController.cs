using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HydroGuard.API.Services;
using HydroGuard.API.ViewModels;

namespace HydroGuard.API.Controllers;

/// <summary>
/// Controller de Medidores - cadastro e listagem de hidrômetros.
/// POST /api/medidores e GET /api/medidores
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize] // Endpoints protegidos - precisa de JWT
public class MedidoresController : ControllerBase
{
  private readonly IMedidorService _medidorService;

  public MedidoresController(IMedidorService medidorService)
  {
    _medidorService = medidorService;
  }

  /// <summary>
  /// Lista medidores com paginação.
  /// GET /api/medidores?page=1&pageSize=10
  /// </summary>
  [HttpGet]
  public async Task<ActionResult<PagedResultViewModel<MedidorViewModel>>> Get(
      [FromQuery] int page = 1,
      [FromQuery] int pageSize = 10)
  {
    var result = await _medidorService.GetPagedAsync(page, pageSize);
    return Ok(result);
  }

  /// <summary>
  /// Cadastra um novo medidor/hidrômetro.
  /// POST /api/medidores
  /// </summary>
  [HttpPost]
  public async Task<ActionResult<MedidorViewModel>> Post([FromBody] MedidorCreateViewModel model)
  {
    // Valida o modelo
    if (!ModelState.IsValid)
    {
      return BadRequest(new ErrorViewModel { Message = "Dados do medidor inválidos" });
    }

    // Obtém ID do usuário autenticado do token JWT
    var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    var result = await _medidorService.CreateAsync(model, userId);

    // Retorna 201 Created
    return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
  }
}
