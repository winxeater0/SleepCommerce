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

    public async Task<Produto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Produtos.FindAsync(new object[] { id }, cancellationToken);
    }

    public async Task<Produto?> GetByIdReadOnlyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Produtos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }

    public async Task<IEnumerable<Produto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Produtos.AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<Produto> Items, int TotalCount)> GetPagedAsync(
        string? nome,
        string? orderBy,
        string? orderDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Produtos.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(nome))
        {
            query = query.Where(p => p.Nome.ToLower().Contains(nome.ToLower()));
        }

        var orderedQuery = ApplyOrdering(query, orderBy, orderDirection);

        var projected = await orderedQuery
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new { Item = p, TotalCount = query.Count() })
            .ToListAsync(cancellationToken);

        var items = projected.Select(x => x.Item);
        var totalCount = projected.FirstOrDefault()?.TotalCount ?? 0;

        return (items, totalCount);
    }

    public async Task AddAsync(Produto produto, CancellationToken cancellationToken = default)
    {
        await _context.Produtos.AddAsync(produto, cancellationToken);
    }

    public void Update(Produto produto)
    {
        _context.Produtos.Update(produto);
    }

    public void Delete(Produto produto)
    {
        _context.Produtos.Remove(produto);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
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
