using SleepCommerce.Application.DTOs;

namespace SleepCommerce.Application.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductResponse>> GetAllAsync(ProductQueryParameters parameters, CancellationToken cancellationToken = default);
    Task<ProductResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ProductResponse> CreateAsync(ProductRequest request, CancellationToken cancellationToken = default);
    Task<ProductResponse?> UpdateAsync(Guid id, ProductRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
