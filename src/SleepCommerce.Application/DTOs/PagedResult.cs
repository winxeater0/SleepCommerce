namespace SleepCommerce.Application.DTOs;

/// <summary>
/// Resultado paginado de uma consulta.
/// </summary>
/// <typeparam name="T">Tipo dos itens retornados.</typeparam>
/// <param name="Items">Itens da página atual.</param>
/// <param name="TotalCount">Total de registros encontrados.</param>
/// <param name="PageNumber">Número da página atual.</param>
/// <param name="PageSize">Tamanho da página.</param>
public record PagedResult<T>(
    IEnumerable<T> Items,
    int TotalCount,
    int PageNumber,
    int PageSize)
{
    /// <summary>Total de páginas disponíveis.</summary>
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
    /// <summary>Indica se existe uma página anterior.</summary>
    public bool HasPrevious => PageNumber > 1;
    /// <summary>Indica se existe uma próxima página.</summary>
    public bool HasNext => PageNumber < TotalPages;
}
