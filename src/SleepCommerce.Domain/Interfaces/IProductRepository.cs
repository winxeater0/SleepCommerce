using SleepCommerce.Domain.Entities;

namespace SleepCommerce.Domain.Interfaces;

public interface IProductRepository
{
    Task<Produto?> GetByIdAsync(Guid id);
    Task<IEnumerable<Produto>> GetAllAsync();
    Task<(IEnumerable<Produto> Items, int TotalCount)> GetPagedAsync(
        string? nome,
        string? orderBy,
        string? orderDirection,
        int pageNumber,
        int pageSize);
    Task AddAsync(Produto produto);
    void Update(Produto produto);
    void Delete(Produto produto);
    Task<int> SaveChangesAsync();
}
