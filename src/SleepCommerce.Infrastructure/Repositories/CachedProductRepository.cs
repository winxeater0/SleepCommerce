using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using SleepCommerce.Domain.Entities;
using SleepCommerce.Domain.Interfaces;
using StackExchange.Redis;

namespace SleepCommerce.Infrastructure.Repositories;

public class CachedProductRepository : IProductRepository
{
    private readonly IProductRepository _inner;
    private readonly IDistributedCache _cache;
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<CachedProductRepository> _logger;

    private static readonly TimeSpan CacheTtlById = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan CacheTtlList = TimeSpan.FromSeconds(30);

    private const string CachePrefix = "SleepCommerce:";
    private const string ProductKeyPrefix = "products:";

    public CachedProductRepository(
        IProductRepository inner,
        IDistributedCache cache,
        IConnectionMultiplexer redis,
        ILogger<CachedProductRepository> logger)
    {
        _inner = inner;
        _cache = cache;
        _redis = redis;
        _logger = logger;
    }

    public async Task<Produto?> GetByIdReadOnlyAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"{ProductKeyPrefix}{id}";

        var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogDebug("Cache hit for product {ProductId}", id);
            return ToEntity(JsonSerializer.Deserialize<ProdutoCacheDto>(cached)!);
        }

        var produto = await _inner.GetByIdReadOnlyAsync(id, cancellationToken);

        if (produto is not null)
        {
            var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheTtlById };
            await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(ToDto(produto)), options, cancellationToken);
        }

        return produto;
    }

    public async Task<(IEnumerable<Produto> Items, int TotalCount)> GetPagedAsync(
        string? nome,
        string? orderBy,
        string? orderDirection,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var hash = ComputeHash($"{nome}|{orderBy}|{orderDirection}|{pageNumber}|{pageSize}");
        var cacheKey = $"{ProductKeyPrefix}page:{hash}";

        var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogDebug("Cache hit for paged query {CacheKey}", cacheKey);
            var result = JsonSerializer.Deserialize<PagedCacheEntry>(cached)!;
            return (result.Items.Select(ToEntity).ToList(), result.TotalCount);
        }

        var (items, totalCount) = await _inner.GetPagedAsync(nome, orderBy, orderDirection, pageNumber, pageSize, cancellationToken);

        var itemsList = items.ToList();
        var entry = new PagedCacheEntry(itemsList.Select(ToDto).ToList(), totalCount);
        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheTtlList };
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(entry), options, cancellationToken);

        return (itemsList, totalCount);
    }

    public async Task<IEnumerable<Produto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        const string cacheKey = $"{ProductKeyPrefix}all";

        var cached = await _cache.GetStringAsync(cacheKey, cancellationToken);
        if (cached is not null)
        {
            _logger.LogDebug("Cache hit for GetAllAsync");
            return JsonSerializer.Deserialize<List<ProdutoCacheDto>>(cached)!.Select(ToEntity).ToList();
        }

        var produtos = (await _inner.GetAllAsync(cancellationToken)).ToList();

        var options = new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = CacheTtlList };
        await _cache.SetStringAsync(cacheKey, JsonSerializer.Serialize(produtos.Select(ToDto).ToList()), options, cancellationToken);

        return produtos;
    }

    public Task<Produto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _inner.GetByIdAsync(id, cancellationToken);

    public Task AddAsync(Produto produto, CancellationToken cancellationToken = default)
        => _inner.AddAsync(produto, cancellationToken);

    public void Update(Produto produto)
        => _inner.Update(produto);

    public void Delete(Produto produto)
        => _inner.Delete(produto);

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var result = await _inner.SaveChangesAsync(cancellationToken);

        await InvalidateCacheAsync();

        return result;
    }

    private async Task InvalidateCacheAsync()
    {
        try
        {
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var pattern = $"{CachePrefix}{ProductKeyPrefix}*";

            var keys = new List<RedisKey>();
            await foreach (var key in server.KeysAsync(pattern: pattern))
            {
                keys.Add(key);
            }

            if (keys.Count > 0)
            {
                var db = _redis.GetDatabase();
                await db.KeyDeleteAsync(keys.ToArray());
                _logger.LogDebug("Invalidated {Count} cache keys", keys.Count);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to invalidate cache");
        }
    }

    private static string ComputeHash(string input)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(input));
        return Convert.ToHexStringLower(bytes)[..16];
    }

    private static ProdutoCacheDto ToDto(Produto p) =>
        new(p.Id, p.Nome, p.Descricao, p.Estoque, p.Valor, p.DataCriacao, p.DataAtualizacao);

    private static Produto ToEntity(ProdutoCacheDto dto) =>
        Produto.Reconstituir(dto.Id, dto.Nome, dto.Descricao, dto.Estoque, dto.Valor, dto.DataCriacao, dto.DataAtualizacao);

    private sealed record ProdutoCacheDto(Guid Id, string Nome, string? Descricao, int Estoque, decimal Valor, DateTime DataCriacao, DateTime? DataAtualizacao);
    private sealed record PagedCacheEntry(List<ProdutoCacheDto> Items, int TotalCount);
}
