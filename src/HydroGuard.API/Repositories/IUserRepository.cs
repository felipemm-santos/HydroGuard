using HydroGuard.API.Models;

namespace HydroGuard.API.Repositories;

/// <summary>
/// Interface do repositório de Usuários (autenticação).
/// </summary>
public interface IUserRepository
{
  // Busca usuário pelo username (para login)
  Task<User?> GetByUsernameAsync(string username);
}
