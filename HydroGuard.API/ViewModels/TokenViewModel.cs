namespace HydroGuard.API.ViewModels;

/// <summary>
/// ViewModel para retorno do token JWT após login bem-sucedido.
/// </summary>
public class TokenViewModel
{
  // Token JWT para autenticação nas requisições protegidas
  public string Token { get; set; } = string.Empty;
}
