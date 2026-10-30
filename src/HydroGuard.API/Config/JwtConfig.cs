using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace HydroGuard.API.Config;

/// <summary>
/// Configuração de autenticação JWT no ASP.NET Core.
/// Registra JWT Bearer como scheme de autenticação padrão.
/// </summary>
public static class JwtConfig
{
  /// <summary>
  /// Adiciona e configura autenticação JWT na aplicação.
  /// </summary>
  public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
  {
    // Lê configurações JWT do appsettings.json
    var jwtKey = configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key não configurada.");
    var jwtIssuer = configuration["Jwt:Issuer"] ?? "HydroGuard";
    var jwtAudience = configuration["Jwt:Audience"] ?? "HydroGuardUsers";

    if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
      throw new InvalidOperationException("Jwt:Key deve ter pelo menos 32 bytes.");

    // Configura autenticação JWT Bearer
    services.AddAuthentication(options =>
    {
      options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
      options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
      options.TokenValidationParameters = new TokenValidationParameters
      {
        // Valida a assinatura do token
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),

        // Valida o emissor (issuer)
        ValidateIssuer = true,
        ValidIssuer = jwtIssuer,

        // Valida a audiência (audience)
        ValidateAudience = true,
        ValidAudience = jwtAudience,

        // Valida expiração do token
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero
      };
    });

    // Configura autorização com roles (Admin, Operador)
    services.AddAuthorization(options =>
    {
      options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
      options.AddPolicy("OperadorOrAdmin", policy => policy.RequireRole("Admin", "Operador"));
    });

    return services;
  }
}
