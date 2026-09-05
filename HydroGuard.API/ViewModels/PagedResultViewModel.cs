namespace HydroGuard.API.ViewModels;

/// <summary>
/// ViewModel genérico para respostas paginadas.
/// Usado em todos os endpoints de listagem (GET com page e pageSize).
/// </summary>
public class PagedResultViewModel<T>
{
  // Página atual (começa em 1)
  public int Page { get; set; }

  // Quantidade de itens por página
  public int PageSize { get; set; }

  // Total de registros no banco (para calcular páginas)
  public int TotalRecords { get; set; }

  // Lista de itens da página atual
  public List<T> Items { get; set; } = new();
}
