namespace SleepCommerce.Application.DTOs;

/// <summary>
/// Dados para criação ou atualização de um produto.
/// </summary>
/// <param name="Nome">Nome do produto.</param>
/// <param name="Descricao">Descrição opcional do produto.</param>
/// <param name="Estoque">Quantidade em estoque.</param>
/// <param name="Valor">Preço unitário do produto.</param>
public record ProductRequest(
    string Nome,
    string? Descricao,
    int Estoque,
    decimal Valor);
