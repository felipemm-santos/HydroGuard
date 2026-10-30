using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using HydroGuard.API.Services;
using HydroGuard.API.ViewModels;

namespace HydroGuard.API.Controllers;

/// <summary>
/// Controller de autenticação - endpoint de login JWT.
/// POST /api/auth/login
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
  private readonly IAuthService _authService;

  public AuthController(IAuthService authService)
  {
    _authService = authService;
  }

  /// <summary>
  /// Autentica usuário e retorna token JWT.
  /// Usuário inicial: admin; senha fornecida por Bootstrap:AdminPassword.
  /// </summary>
  [HttpPost("login")]
  [AllowAnonymous] // Login não precisa de autenticação
  public async Task<ActionResult<TokenViewModel>> Login([FromBody] LoginViewModel model)
  {
    // Valida o modelo (DataAnnotations)
    if (!ModelState.IsValid)
    {
      return BadRequest(new ErrorViewModel { Message = "Dados de login inválidos" });
    }

    // Tenta autenticar
    var result = await _authService.LoginAsync(model);

    if (result == null)
    {
      return Unauthorized(new ErrorViewModel { Message = "Credenciais inválidas" });
    }

  // Retorna token JWT
    return Ok(result);
  }
}
