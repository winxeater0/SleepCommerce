using System.Diagnostics;
using SleepCommerce.Application.Diagnostics;
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

    public async Task<PagedResult<ProductResponse>> GetAllAsync(ProductQueryParameters parameters, CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            parameters.Nome,
            parameters.OrderBy,
            parameters.OrderDirection,
            parameters.PageNumber,
            parameters.PageSize,
            cancellationToken);

        var responses = items.Select(MapToResponse);

        return new PagedResult<ProductResponse>(
            responses,
            totalCount,
            parameters.PageNumber,
            parameters.PageSize);
    }

    public async Task<ProductResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var produto = await _repository.GetByIdReadOnlyAsync(id, cancellationToken);
        return produto is null ? null : MapToResponse(produto);
    }

    public async Task<ProductResponse> CreateAsync(ProductRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Default.StartActivity("ProductService.Create");

        var produto = new Produto(request.Nome, request.Descricao, request.Estoque, request.Valor);

        activity?.SetTag("product.id", produto.Id.ToString());

        await _repository.AddAsync(produto, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);
        return MapToResponse(produto);
    }

    public async Task<ProductResponse?> UpdateAsync(Guid id, ProductRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Default.StartActivity("ProductService.Update");
        activity?.SetTag("product.id", id.ToString());

        var produto = await _repository.GetByIdAsync(id, cancellationToken);
        if (produto is null) return null;

        produto.Atualizar(request.Nome, request.Descricao, request.Estoque, request.Valor);
        _repository.Update(produto);
        await _repository.SaveChangesAsync(cancellationToken);
        return MapToResponse(produto);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var activity = ActivitySources.Default.StartActivity("ProductService.Delete");
        activity?.SetTag("product.id", id.ToString());

        var produto = await _repository.GetByIdAsync(id, cancellationToken);
        if (produto is null) return false;

        _repository.Delete(produto);
        await _repository.SaveChangesAsync(cancellationToken);
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
