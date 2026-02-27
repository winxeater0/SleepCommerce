using Microsoft.EntityFrameworkCore;
using SleepCommerce.Domain.Entities;
using SleepCommerce.Domain.Interfaces;
using SleepCommerce.Infrastructure.Data;

namespace SleepCommerce.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _context;

    public ProductRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Produto?> GetByIdAsync(Guid id)
    {
        return await _context.Produtos.FindAsync(id);
    }

    public async Task<IEnumerable<Produto>> GetAllAsync()
    {
        return await _context.Produtos.ToListAsync();
    }

    public async Task<(IEnumerable<Produto> Items, int TotalCount)> GetPagedAsync(
        string? nome,
        string? orderBy,
        string? orderDirection,
        int pageNumber,
        int pageSize)
    {
        var query = _context.Produtos.AsQueryable();

        if (!string.IsNullOrWhiteSpace(nome))
        {
            query = query.Where(p => p.Nome.ToLower().Contains(nome.ToLower()));
        }

        var totalCount = await query.CountAsync();

        query = ApplyOrdering(query, orderBy, orderDirection);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task AddAsync(Produto produto)
    {
        await _context.Produtos.AddAsync(produto);
    }

    public void Update(Produto produto)
    {
        _context.Produtos.Update(produto);
    }

    public void Delete(Produto produto)
    {
        _context.Produtos.Remove(produto);
    }

    public async Task<int> SaveChangesAsync()
    {
        return await _context.SaveChangesAsync();
    }

    private static IQueryable<Produto> ApplyOrdering(IQueryable<Produto> query, string? orderBy, string? orderDirection)
    {
        var isDescending = string.Equals(orderDirection, "desc", StringComparison.OrdinalIgnoreCase);

        return orderBy?.ToLower() switch
        {
            "valor" => isDescending ? query.OrderByDescending(p => p.Valor) : query.OrderBy(p => p.Valor),
            "estoque" => isDescending ? query.OrderByDescending(p => p.Estoque) : query.OrderBy(p => p.Estoque),
            "datacriacao" => isDescending ? query.OrderByDescending(p => p.DataCriacao) : query.OrderBy(p => p.DataCriacao),
            _ => isDescending ? query.OrderByDescending(p => p.Nome) : query.OrderBy(p => p.Nome),
        };
    }
}
