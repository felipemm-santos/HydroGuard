using System.ComponentModel.DataAnnotations;

namespace HydroGuard.API.ViewModels;

/// <summary>
/// ViewModel para cadastro de um novo medidor/hidrômetro.
/// Usado no POST /api/medidores com validações via DataAnnotations.
/// </summary>
public class MedidorCreateViewModel
{
  // Nome do medidor - campo obrigatório
  [Required(ErrorMessage = "O nome do medidor é obrigatório")]
  [StringLength(100, ErrorMessage = "O nome deve ter no máximo 100 caracteres")]
  public string Nome { get; set; } = string.Empty;

  // Serial único do medidor - campo obrigatório e único no banco
  [Required(ErrorMessage = "O serial do medidor é obrigatório")]
  [StringLength(50, ErrorMessage = "O serial deve ter no máximo 50 caracteres")]
  public string Serial { get; set; } = string.Empty;

  // Localização do medidor - campo obrigatório
  [Required(ErrorMessage = "A localização é obrigatória")]
  [StringLength(200, ErrorMessage = "A localização deve ter no máximo 200 caracteres")]
  public string Localizacao { get; set; } = string.Empty;
}
