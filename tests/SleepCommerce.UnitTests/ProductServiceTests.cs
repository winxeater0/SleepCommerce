using FluentAssertions;
using Moq;
using SleepCommerce.Application.DTOs;
using SleepCommerce.Application.Services;
using SleepCommerce.Domain.Entities;
using SleepCommerce.Domain.Interfaces;

namespace SleepCommerce.UnitTests;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _repositoryMock;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _repositoryMock = new Mock<IProductRepository>();
        _service = new ProductService(_repositoryMock.Object);
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnPagedResult()
    {
        var produtos = new List<Produto>
        {
            new("Produto 1", "Desc 1", 10, 99.90m),
            new("Produto 2", "Desc 2", 20, 199.90m)
        };
        _repositoryMock.Setup(r => r.GetPagedAsync(null, "nome", "asc", 1, 10))
            .ReturnsAsync((produtos.AsEnumerable(), 2));

        var result = await _service.GetAllAsync(new ProductQueryParameters());

        result.TotalCount.Should().Be(2);
        result.Items.Should().HaveCount(2);
        result.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExists_ShouldReturnProduct()
    {
        var produto = new Produto("Notebook", "Desc", 5, 4999.99m);
        _repositoryMock.Setup(r => r.GetByIdAsync(produto.Id))
            .ReturnsAsync(produto);

        var result = await _service.GetByIdAsync(produto.Id);

        result.Should().NotBeNull();
        result!.Nome.Should().Be("Notebook");
        result.Valor.Should().Be(4999.99m);
    }

    [Fact]
    public async Task GetByIdAsync_WhenNotExists_ShouldReturnNull()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Produto?)null);

        var result = await _service.GetByIdAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateAsync_ShouldCreateAndReturnProduct()
    {
        var request = new ProductRequest("Mouse", "Mouse gamer", 100, 149.90m);
        _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Produto>())).Returns(Task.CompletedTask);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _service.CreateAsync(request);

        result.Nome.Should().Be("Mouse");
        result.Estoque.Should().Be(100);
        result.Valor.Should().Be(149.90m);
        _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Produto>()), Times.Once);
        _repositoryMock.Verify(r => r.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenExists_ShouldUpdateAndReturnProduct()
    {
        var produto = new Produto("Mouse", "Desc", 10, 99.90m);
        _repositoryMock.Setup(r => r.GetByIdAsync(produto.Id)).ReturnsAsync(produto);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var request = new ProductRequest("Mouse Atualizado", "Nova desc", 20, 149.90m);
        var result = await _service.UpdateAsync(produto.Id, request);

        result.Should().NotBeNull();
        result!.Nome.Should().Be("Mouse Atualizado");
        result.Estoque.Should().Be(20);
        result.Valor.Should().Be(149.90m);
        _repositoryMock.Verify(r => r.Update(produto), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenNotExists_ShouldReturnNull()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Produto?)null);

        var result = await _service.UpdateAsync(Guid.NewGuid(), new ProductRequest("X", null, 0, 1));

        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WhenExists_ShouldReturnTrue()
    {
        var produto = new Produto("Mouse", "Desc", 10, 99.90m);
        _repositoryMock.Setup(r => r.GetByIdAsync(produto.Id)).ReturnsAsync(produto);
        _repositoryMock.Setup(r => r.SaveChangesAsync()).ReturnsAsync(1);

        var result = await _service.DeleteAsync(produto.Id);

        result.Should().BeTrue();
        _repositoryMock.Verify(r => r.Delete(produto), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotExists_ShouldReturnFalse()
    {
        _repositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Produto?)null);

        var result = await _service.DeleteAsync(Guid.NewGuid());

        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_WithFilter_ShouldPassParametersToRepository()
    {
        var parameters = new ProductQueryParameters
        {
            Nome = "mouse",
            OrderBy = "valor",
            OrderDirection = "desc",
            PageNumber = 2,
            PageSize = 5
        };
        _repositoryMock.Setup(r => r.GetPagedAsync("mouse", "valor", "desc", 2, 5))
            .ReturnsAsync((Enumerable.Empty<Produto>(), 0));

        var result = await _service.GetAllAsync(parameters);

        result.TotalCount.Should().Be(0);
        result.PageNumber.Should().Be(2);
        result.PageSize.Should().Be(5);
    }
}
