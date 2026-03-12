using System.Text;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Moq;
using SleepCommerce.Domain.Entities;
using SleepCommerce.Domain.Interfaces;
using SleepCommerce.Infrastructure.Repositories;
using StackExchange.Redis;

namespace SleepCommerce.UnitTests;

public class CachedProductRepositoryTests
{
    private readonly Mock<IProductRepository> _innerMock = new();
    private readonly Mock<IDistributedCache> _cacheMock = new();
    private readonly Mock<IConnectionMultiplexer> _redisMock = new();
    private readonly Mock<ILogger<CachedProductRepository>> _loggerMock = new();
    private readonly CachedProductRepository _sut;

    public CachedProductRepositoryTests()
    {
        _sut = new CachedProductRepository(
            _innerMock.Object,
            _cacheMock.Object,
            _redisMock.Object,
            _loggerMock.Object);
    }

    private static Produto CreateProduto(Guid? id = null)
    {
        return Produto.Reconstituir(
            id ?? Guid.NewGuid(),
            "Produto Teste",
            "Descricao",
            10,
            99.90m,
            DateTime.UtcNow,
            null);
    }

    private static byte[] SerializeAsDto(Produto p)
    {
        var dto = new { p.Id, p.Nome, p.Descricao, p.Estoque, p.Valor, p.DataCriacao, p.DataAtualizacao };
        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(dto));
    }

    [Fact]
    public async Task GetByIdReadOnlyAsync_CacheHit_ReturnsFromCacheWithoutCallingInner()
    {
        // Arrange
        var id = Guid.NewGuid();
        var produto = CreateProduto(id);
        var bytes = SerializeAsDto(produto);

        _cacheMock.Setup(c => c.GetAsync($"products:{id}", It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        // Act
        var result = await _sut.GetByIdReadOnlyAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        _innerMock.Verify(r => r.GetByIdReadOnlyAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdReadOnlyAsync_CacheMiss_CallsInnerAndPopulatesCache()
    {
        // Arrange
        var id = Guid.NewGuid();
        var produto = CreateProduto(id);

        _cacheMock.Setup(c => c.GetAsync($"products:{id}", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        _innerMock.Setup(r => r.GetByIdReadOnlyAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(produto);

        // Act
        var result = await _sut.GetByIdReadOnlyAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(id);
        _innerMock.Verify(r => r.GetByIdReadOnlyAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.SetAsync(
            $"products:{id}",
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByIdReadOnlyAsync_CacheMiss_ProductNotFound_DoesNotPopulateCache()
    {
        // Arrange
        var id = Guid.NewGuid();

        _cacheMock.Setup(c => c.GetAsync($"products:{id}", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        _innerMock.Setup(r => r.GetByIdReadOnlyAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Produto?)null);

        // Act
        var result = await _sut.GetByIdReadOnlyAsync(id);

        // Assert
        result.Should().BeNull();
        _cacheMock.Verify(c => c.SetAsync(
            It.IsAny<string>(),
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_AlwaysCallsInner_NeverUsesCache()
    {
        // Arrange
        var id = Guid.NewGuid();
        var produto = CreateProduto(id);

        _innerMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(produto);

        // Act
        var result = await _sut.GetByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        _innerMock.Verify(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.GetAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetAllAsync_CacheMiss_CallsInnerAndPopulatesCache()
    {
        // Arrange
        var produtos = new List<Produto> { CreateProduto(), CreateProduto() };

        _cacheMock.Setup(c => c.GetAsync("products:all", It.IsAny<CancellationToken>()))
            .ReturnsAsync((byte[]?)null);

        _innerMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(produtos);

        // Act
        var result = await _sut.GetAllAsync();

        // Assert
        result.Should().HaveCount(2);
        _innerMock.Verify(r => r.GetAllAsync(It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.SetAsync(
            "products:all",
            It.IsAny<byte[]>(),
            It.IsAny<DistributedCacheEntryOptions>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetPagedAsync_CacheHit_ReturnsFromCacheWithoutCallingInner()
    {
        // Arrange
        var produto = CreateProduto();
        var entry = new { Items = new[] { new { produto.Id, produto.Nome, produto.Descricao, produto.Estoque, produto.Valor, produto.DataCriacao, produto.DataAtualizacao } }, TotalCount = 1 };
        var bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(entry));

        _cacheMock.Setup(c => c.GetAsync(It.Is<string>(k => k.StartsWith("products:page:")), It.IsAny<CancellationToken>()))
            .ReturnsAsync(bytes);

        // Act
        var (items, totalCount) = await _sut.GetPagedAsync(null, null, null, 1, 10);

        // Assert
        items.Should().HaveCount(1);
        totalCount.Should().Be(1);
        _innerMock.Verify(r => r.GetPagedAsync(
            It.IsAny<string?>(), It.IsAny<string?>(), It.IsAny<string?>(),
            It.IsAny<int>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SaveChangesAsync_CallsInnerAndInvalidatesCache()
    {
        // Arrange
        _innerMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var serverMock = new Mock<IServer>();
        serverMock.Setup(s => s.KeysAsync(It.IsAny<int>(), It.IsAny<RedisValue>(), It.IsAny<int>(), It.IsAny<long>(), It.IsAny<int>(), It.IsAny<CommandFlags>()))
            .Returns(AsyncEnumerable(new RedisKey[] { "SleepCommerce:products:123", "SleepCommerce:products:all" }));

        var dbMock = new Mock<IDatabase>();
        dbMock.Setup(d => d.KeyDeleteAsync(It.IsAny<RedisKey[]>(), It.IsAny<CommandFlags>()))
            .ReturnsAsync(2);

        _redisMock.Setup(r => r.GetEndPoints(It.IsAny<bool>()))
            .Returns(new System.Net.EndPoint[] { new System.Net.DnsEndPoint("localhost", 6379) });
        _redisMock.Setup(r => r.GetServer(It.IsAny<System.Net.EndPoint>(), It.IsAny<object>()))
            .Returns(serverMock.Object);
        _redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
            .Returns(dbMock.Object);

        // Act
        var result = await _sut.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
        _innerMock.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        dbMock.Verify(d => d.KeyDeleteAsync(It.IsAny<RedisKey[]>(), It.IsAny<CommandFlags>()), Times.Once);
    }

    [Fact]
    public async Task SaveChangesAsync_CacheInvalidationFails_StillReturnsResult()
    {
        // Arrange
        _innerMock.Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        _redisMock.Setup(r => r.GetEndPoints(It.IsAny<bool>()))
            .Throws(new RedisConnectionException(ConnectionFailureType.UnableToConnect, "Connection failed"));

        // Act
        var result = await _sut.SaveChangesAsync();

        // Assert
        result.Should().Be(1);
    }

#pragma warning disable CS1998
    private static async IAsyncEnumerable<RedisKey> AsyncEnumerable(RedisKey[] keys)
    {
        foreach (var key in keys)
            yield return key;
    }
#pragma warning restore CS1998
}
