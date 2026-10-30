using System.Net;
using System.Text.Json;
using HydroGuard.API.ViewModels;

namespace HydroGuard.API.Middleware;

/// <summary>
/// Middleware global de tratamento de erros.
/// Captura exceções e retorna JSON padronizado com status codes corretos.
/// </summary>
public class ExceptionHandlingMiddleware
{
  // Próximo middleware na pipeline
  private readonly RequestDelegate _next;
  private readonly ILogger<ExceptionHandlingMiddleware> _logger;

  public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
  {
    _next = next;
    _logger = logger;
  }

  /// <summary>
  /// Executa o middleware - tenta processar a requisição e captura erros.
  /// </summary>
  public async Task InvokeAsync(HttpContext context)
  {
    try
    {
      // Passa a requisição ao próximo middleware/controller
      await _next(context);
    }
    catch (Exception ex)
    {
      // Log do erro para diagnóstico
      _logger.LogError(ex, "Erro não tratado: {Message}", ex.Message);

      // Trata a exceção e retorna JSON padronizado
      await HandleExceptionAsync(context, ex);
    }
  }

  /// <summary>
  /// Mapeia exceções para status HTTP e mensagens padronizadas.
  /// </summary>
  private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
  {
    // Define status code e mensagem baseado no tipo de exceção
    var (statusCode, message) = MapException(exception);

    // Monta resposta JSON padronizada
    var response = new ErrorViewModel
    {
      Error = true,
      Message = message
    };

    // Configura a resposta HTTP
    context.Response.ContentType = "application/json";
    context.Response.StatusCode = statusCode;

    // Serializa e envia o JSON
    var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
    {
      PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    });

    await context.Response.WriteAsync(json);
  }

  /// <summary>
  /// Mapeia tipos de exceção para status HTTP (400, 401, 404, 500).
  /// </summary>
  private static (int statusCode, string message) MapException(Exception exception)
  {
    switch (exception)
    {
      // 400 Bad Request - dados inválidos ou regra de negócio
      case ArgumentException or InvalidOperationException:
        return ((int)HttpStatusCode.BadRequest, exception.Message);

      // 404 Not Found - recurso não encontrado
      case KeyNotFoundException:
        return ((int)HttpStatusCode.NotFound, exception.Message);

      // 401 Unauthorized - não autenticado
      case UnauthorizedAccessException:
        return ((int)HttpStatusCode.Unauthorized, exception.Message);

      // 500 Internal Server Error - erro genérico
      default:
        return ((int)HttpStatusCode.InternalServerError, "Erro interno do servidor");
    }
  }
}
