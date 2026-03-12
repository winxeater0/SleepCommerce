using SleepCommerce.Domain.Entities;

namespace SleepCommerce.Domain.Interfaces;

public interface IProductRepository
{
    Task<Produto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Produto?> GetByIdReadOnlyAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IEnumerable<Produto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<(IEnumerable<Produto> Items, int TotalCount)> GetPagedAsync(
        string? nome,
        string? orderBy,
        string? orderDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task AddAsync(Produto produto, CancellationToken cancellationToken = default);
    void Update(Produto produto);
    void Delete(Produto produto);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
