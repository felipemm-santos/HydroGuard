using HydroGuard.API.ViewModels;

namespace HydroGuard.API.Services;

/// <summary>
/// Interface do serviço de autenticação JWT.
/// </summary>
public interface IAuthService
{
  Task<TokenViewModel?> LoginAsync(LoginViewModel model);
}
