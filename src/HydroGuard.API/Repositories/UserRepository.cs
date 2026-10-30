using Microsoft.EntityFrameworkCore;
using HydroGuard.API.Data;
using HydroGuard.API.Models;

namespace HydroGuard.API.Repositories;

/// <summary>
/// Implementação do repositório de Usuários.
/// </summary>
public class UserRepository : IUserRepository
{
  private readonly HydroGuardDbContext _context;

  public UserRepository(HydroGuardDbContext context)
  {
    _context = context;
  }

  /// <summary>
  /// Busca um usuário pelo username para autenticação.
  /// </summary>
  public async Task<User?> GetByUsernameAsync(string username)
  {
    return await _context.Users
        .FirstOrDefaultAsync(u => u.Username == username);
  }
}
