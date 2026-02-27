namespace SleepCommerce.Application.DTOs;

/// <summary>
/// Representação de um produto retornado pela API.
/// </summary>
/// <param name="Id">Identificador único do produto.</param>
/// <param name="Nome">Nome do produto.</param>
/// <param name="Descricao">Descrição do produto.</param>
/// <param name="Estoque">Quantidade em estoque.</param>
/// <param name="Valor">Preço unitário.</param>
/// <param name="DataCriacao">Data de criação do registro.</param>
/// <param name="DataAtualizacao">Data da última atualização.</param>
public record ProductResponse(
    Guid Id,
    string Nome,
    string? Descricao,
    int Estoque,
    decimal Valor,
    DateTime DataCriacao,
    DateTime? DataAtualizacao);
