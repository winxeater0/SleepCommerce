using SleepCommerce.Application.DTOs;

namespace SleepCommerce.Application.Interfaces;

public interface IProductService
{
    Task<PagedResult<ProductResponse>> GetAllAsync(ProductQueryParameters parameters);
    Task<ProductResponse?> GetByIdAsync(Guid id);
    Task<ProductResponse> CreateAsync(ProductRequest request);
    Task<ProductResponse?> UpdateAsync(Guid id, ProductRequest request);
    Task<bool> DeleteAsync(Guid id);
}
