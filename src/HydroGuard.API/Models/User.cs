namespace HydroGuard.API.Models;

/// <summary>
/// Modelo que representa um usuário do sistema HydroGuard.
/// Cada usuário pode cadastrar vários medidores de água.
/// </summary>
public class User
{
  // Identificador único do usuário (chave primária)
  public int Id { get; set; }

  // Nome de login do usuário (usado na autenticação)
  public string Username { get; set; } = string.Empty;

  // Senha hasheada com BCrypt (nunca salvar senha em texto puro!)
  public string PasswordHash { get; set; } = string.Empty;

  // Role do usuário: Admin ou Operador (controle de acesso)
  public string Role { get; set; } = "Operador";

  // Relacionamento: um usuário tem vários medidores
  public List<Medidor> Medidores { get; set; } = new();
}
