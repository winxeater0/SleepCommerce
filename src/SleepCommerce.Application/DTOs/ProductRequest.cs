namespace SleepCommerce.Application.DTOs;

public record ProductRequest(
    string Nome,
    string? Descricao,
    int Estoque,
    decimal Valor);
