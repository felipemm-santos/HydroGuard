using System.ComponentModel.DataAnnotations;

namespace HydroGuard.API.ViewModels;

/// <summary>
/// ViewModel para registro de consumo de água.
/// Usado no POST /api/consumos.
/// </summary>
public class ConsumoCreateViewModel
{
  // ID do medidor que registrou o consumo - obrigatório
  [Required(ErrorMessage = "O ID do medidor é obrigatório")]
  public int MedidorId { get; set; }

  // Litros consumidos - não pode ser negativo
  [Required(ErrorMessage = "O consumo em litros é obrigatório")]
  [Range(0, double.MaxValue, ErrorMessage = "O consumo não pode ser negativo")]
  public double LitrosConsumidos { get; set; }
}
