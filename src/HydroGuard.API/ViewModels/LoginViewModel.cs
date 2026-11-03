using System.ComponentModel.DataAnnotations;

namespace HydroGuard.API.ViewModels;

/// <summary>
/// ViewModel para login de usuário.
/// Usado no POST /api/auth/login.
/// </summary>
public class LoginViewModel
{
  // Nome de usuário - obrigatório
  [Required(ErrorMessage = "O username é obrigatório")]
  public string Username { get; set; } = string.Empty;

  // Senha - obrigatória
  [Required(ErrorMessage = "A senha é obrigatória")]
  public string Password { get; set; } = string.Empty;
}
