using Microsoft.EntityFrameworkCore;
using HydroGuard.API.Config;
using HydroGuard.API.Data;
using HydroGuard.API.Middleware;
using HydroGuard.API.Repositories;
using HydroGuard.API.Services;

// Cria o builder da aplicação ASP.NET Core
var builder = WebApplication.CreateBuilder(args);

// ===== CONFIGURAÇÃO DE SERVIÇOS (DI Container) =====

// Adiciona controllers da API
builder.Services.AddControllers();

// Configura Swagger para documentação da API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
  c.SwaggerDoc("v1", new() { Title = "HydroGuard API", Version = "v1", Description = "API de monitoramento de consumo de água - ESG" });
});

// Configura Entity Framework com SQL Server
// A factory de testes registra seu próprio provider InMemory.
if (!builder.Environment.IsEnvironment("Testing"))
{
  builder.Services.AddDbContext<HydroGuardDbContext>(options =>
      options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}

// Configura autenticação JWT
builder.Services.AddJwtAuthentication(builder.Configuration);

// Registra Repositories (padrão Repository)
builder.Services.AddScoped<IMedidorRepository, MedidorRepository>();
builder.Services.AddScoped<IConsumoRepository, ConsumoRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAlertaRepository, AlertaRepository>();

// Registra Services (lógica de negócio)
builder.Services.AddScoped<IMedidorService, MedidorService>();
builder.Services.AddScoped<IConsumoService, ConsumoService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAlertaService, AlertaService>();
builder.Services.AddScoped<IEsgService, EsgService>();
builder.Services.AddScoped<IRelatorioService, RelatorioService>();

// Configura logging
builder.Logging.AddConsole();

// ===== CONSTRUIR A APLICAÇÃO =====
var app = builder.Build();

// ===== PIPELINE DE MIDDLEWARE =====

// Middleware global de tratamento de erros (primeiro na pipeline)
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Swagger disponível em desenvolvimento e produção
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "HydroGuard API v1"));

// Habilita arquivos estáticos (frontend HTML/CSS/JS)
app.UseDefaultFiles();
app.UseStaticFiles();

// Autenticação e autorização JWT
app.UseAuthentication();
app.UseAuthorization();

// Mapeia os controllers
app.MapControllers();

// Readiness inclui conexão real com o banco. Não expõe credenciais nem exceções.
app.MapGet("/healthz", async (HydroGuardDbContext db, IConfiguration config, IWebHostEnvironment env) =>
{
  try
  {
    if (!await db.Database.CanConnectAsync())
      return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    return Results.Ok(new { status = "healthy", environment = env.EnvironmentName,
      revision = config["HydroGuard:Revision"] ?? "local" });
  }
  catch { return Results.StatusCode(StatusCodes.Status503ServiceUnavailable); }
}).AllowAnonymous();
app.MapGet("/livez", () => Results.Ok(new { status = "alive" })).AllowAnonymous();

// ===== INICIALIZAÇÃO DO BANCO DE DADOS =====
// Em ambiente de testes (xUnit), o seed é feito pela WebApplicationFactory
if (!app.Environment.IsEnvironment("Testing"))
{
  using var scope = app.Services.CreateScope();
  var context = scope.ServiceProvider.GetRequiredService<HydroGuardDbContext>();

  // Aplica migrations automaticamente ao iniciar
  await context.Database.MigrateAsync();

  // Popula dados iniciais (seed)
  var adminPassword = builder.Configuration["Bootstrap:AdminPassword"];
  if (string.IsNullOrWhiteSpace(adminPassword) || adminPassword.Length < 12)
    throw new InvalidOperationException("Bootstrap:AdminPassword deve ter pelo menos 12 caracteres.");
  var seedDemo = builder.Configuration.GetValue<bool>("Bootstrap:SeedDemoData");
  if (app.Environment.IsProduction() && seedDemo)
    throw new InvalidOperationException("Dados demonstrativos não são permitidos em Production.");
  await DbSeeder.SeedAsync(context, adminPassword, seedDemo);
}

// Inicia a aplicação
app.Run();

// Classe parcial para testes de integração (WebApplicationFactory)
public partial class Program { }
