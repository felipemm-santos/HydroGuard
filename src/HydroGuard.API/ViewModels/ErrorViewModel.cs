namespace HydroGuard.API.ViewModels;

/// <summary>
/// ViewModel padronizado para respostas de erro da API.
/// Usado pelo middleware de tratamento de erros.
/// </summary>
public class ErrorViewModel
{
  // Indica que houve erro (sempre true em respostas de erro)
  public bool Error { get; set; } = true;

  // Mensagem descritiva do erro
  public string Message { get; set; } = string.Empty;
}
