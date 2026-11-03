namespace HydroGuard.API.ViewModels;

/// <summary>
/// ViewModel para retorno de um registro de consumo.
/// Inclui a data que foi salva automaticamente no POST.
/// </summary>
public class ConsumoViewModel
{
  // ID do registro
  public int Id { get; set; }

  // ID do medidor vinculado
  public int MedidorId { get; set; }

  // Litros consumidos
  public double LitrosConsumidos { get; set; }

  // Data do registro (automática)
  public DateTime DataRegistro { get; set; }
}
