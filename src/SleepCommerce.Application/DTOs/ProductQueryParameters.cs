namespace SleepCommerce.Application.DTOs;

public record ProductQueryParameters
{
    public string? Nome { get; init; }
    public string? OrderBy { get; init; } = "nome";
    public string? OrderDirection { get; init; } = "asc";
    public int PageNumber { get; init; } = 1;
    public int PageSize { get; init; } = 10;
}
