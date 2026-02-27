using SleepCommerce.Application.DTOs;
using SleepCommerce.Application.Interfaces;
using SleepCommerce.Domain.Entities;
using SleepCommerce.Domain.Interfaces;

namespace SleepCommerce.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;

    public ProductService(IProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<ProductResponse>> GetAllAsync(ProductQueryParameters parameters)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            parameters.Nome,
            parameters.OrderBy,
            parameters.OrderDirection,
            parameters.PageNumber,
            parameters.PageSize);

        var responses = items.Select(MapToResponse);

        return new PagedResult<ProductResponse>(
            responses,
            totalCount,
            parameters.PageNumber,
            parameters.PageSize);
    }

    public async Task<ProductResponse?> GetByIdAsync(Guid id)
    {
        var produto = await _repository.GetByIdAsync(id);
        return produto is null ? null : MapToResponse(produto);
    }

    public async Task<ProductResponse> CreateAsync(ProductRequest request)
    {
        var produto = new Produto(request.Nome, request.Descricao, request.Estoque, request.Valor);
        await _repository.AddAsync(produto);
        await _repository.SaveChangesAsync();
        return MapToResponse(produto);
    }

    public async Task<ProductResponse?> UpdateAsync(Guid id, ProductRequest request)
    {
        var produto = await _repository.GetByIdAsync(id);
        if (produto is null) return null;

        produto.Atualizar(request.Nome, request.Descricao, request.Estoque, request.Valor);
        _repository.Update(produto);
        await _repository.SaveChangesAsync();
        return MapToResponse(produto);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var produto = await _repository.GetByIdAsync(id);
        if (produto is null) return false;

        _repository.Delete(produto);
        await _repository.SaveChangesAsync();
        return true;
    }

    private static ProductResponse MapToResponse(Produto produto)
    {
        return new ProductResponse(
            produto.Id,
            produto.Nome,
            produto.Descricao,
            produto.Estoque,
            produto.Valor,
            produto.DataCriacao,
            produto.DataAtualizacao);
    }
}
