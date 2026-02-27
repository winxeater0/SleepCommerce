namespace SleepCommerce.Application.DTOs;

public record ProductResponse(
    Guid Id,
    string Nome,
    string? Descricao,
    int Estoque,
    decimal Valor,
    DateTime DataCriacao,
    DateTime? DataAtualizacao);
