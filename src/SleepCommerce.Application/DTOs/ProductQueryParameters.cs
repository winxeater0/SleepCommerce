namespace SleepCommerce.Application.DTOs;

/// <summary>
/// Parâmetros de consulta para listagem de produtos.
/// </summary>
public record ProductQueryParameters
{
    /// <summary>Filtro por nome (contém).</summary>
    public string? Nome { get; init; }
    /// <summary>Campo de ordenação (ex: nome, valor, estoque).</summary>
    public string? OrderBy { get; init; } = "nome";
    /// <summary>Direção da ordenação: asc ou desc.</summary>
    public string? OrderDirection { get; init; } = "asc";
    /// <summary>Número da página (padrão: 1).</summary>
    public int PageNumber { get; init; } = 1;
    /// <summary>Itens por página (padrão: 10).</summary>
    public int PageSize { get; init; } = 10;
}
