namespace HydroGuard.API.ViewModels;

/// <summary>
/// ViewModel para retorno de dados de um medidor na listagem.
/// Não expõe dados sensíveis, apenas informações do medidor.
/// </summary>
public class MedidorViewModel
{
  // ID do medidor
  public int Id { get; set; }

  // Nome descritivo
  public string Nome { get; set; } = string.Empty;

  // Serial único
  public string Serial { get; set; } = string.Empty;

  // Localização física
  public string Localizacao { get; set; } = string.Empty;
}
