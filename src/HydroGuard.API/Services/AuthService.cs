using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using HydroGuard.API.Repositories;
using HydroGuard.API.ViewModels;

namespace HydroGuard.API.Services;

/// <summary>
/// Serviço de autenticação - valida credenciais e gera token JWT.
/// </summary>
public class AuthService : IAuthService
{
  private readonly IUserRepository _userRepository;
  private readonly IConfiguration _configuration;

  public AuthService(IUserRepository userRepository, IConfiguration configuration)
  {
    _userRepository = userRepository;
    _configuration = configuration;
  }

  /// <summary>
  /// Autentica usuário e retorna token JWT se credenciais válidas.
  /// </summary>
  public async Task<TokenViewModel?> LoginAsync(LoginViewModel model)
  {
    // Busca usuário pelo username
    var user = await _userRepository.GetByUsernameAsync(model.Username);
    if (user == null)
    {
      return null; // Usuário não encontrado
    }

    // Verifica senha com BCrypt (compara hash)
    if (!BCrypt.Net.BCrypt.Verify(model.Password, user.PasswordHash))
    {
      return null; // Senha incorreta
    }

    // Gera o token JWT
    var token = GenerateJwtToken(user);

    return new TokenViewModel { Token = token };
  }

  /// <summary>
  /// Gera token JWT com claims do usuário (id, username, role).
  /// </summary>
  private string GenerateJwtToken(Models.User user)
  {
    // Lê configurações JWT do appsettings.json
    var jwtKey = _configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key não configurada.");
    var jwtIssuer = _configuration["Jwt:Issuer"] ?? "HydroGuard";
    var jwtAudience = _configuration["Jwt:Audience"] ?? "HydroGuardUsers";

    // Cria chave de segurança
    var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
    var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

    // Claims do token (informações do usuário)
    var claims = new[]
    {
      new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
      new Claim(ClaimTypes.Name, user.Username),
      new Claim(ClaimTypes.Role, user.Role)
    };

    // Monta o token com expiração de 24 horas
    var token = new JwtSecurityToken(
        issuer: jwtIssuer,
        audience: jwtAudience,
        claims: claims,
        expires: DateTime.UtcNow.AddHours(24),
        signingCredentials: credentials
    );

    // Retorna o token em formato string
    return new JwtSecurityTokenHandler().WriteToken(token);
  }
}
