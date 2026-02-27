using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using SleepCommerce.Application.DTOs;

namespace SleepCommerce.IntegrationTests;

public class ProductsControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ProductsControllerTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetAll_ShouldReturnSeededProducts()
    {
        var response = await _client.GetAsync("/api/products");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ProductResponse>>();
        result.Should().NotBeNull();
        result!.TotalCount.Should().BeGreaterThanOrEqualTo(5);
    }

    [Fact]
    public async Task Create_ShouldReturnCreatedProduct()
    {
        var request = new ProductRequest("Produto Teste", "Descrição teste", 50, 199.99m);

        var response = await _client.PostAsJsonAsync("/api/products", request);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
        product.Should().NotBeNull();
        product!.Nome.Should().Be("Produto Teste");
        product.Valor.Should().Be(199.99m);
    }

    [Fact]
    public async Task Create_WithInvalidData_ShouldReturnBadRequest()
    {
        var request = new ProductRequest("", null, -1, 0);

        var response = await _client.PostAsJsonAsync("/api/products", request);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetById_AfterCreate_ShouldReturnProduct()
    {
        var request = new ProductRequest("Produto GetById", "Desc", 10, 50.00m);
        var createResponse = await _client.PostAsJsonAsync("/api/products", request);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();

        var response = await _client.GetAsync($"/api/products/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
        product!.Nome.Should().Be("Produto GetById");
    }

    [Fact]
    public async Task GetById_WithInvalidId_ShouldReturnNotFound()
    {
        var response = await _client.GetAsync($"/api/products/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Update_ShouldReturnUpdatedProduct()
    {
        var createRequest = new ProductRequest("Produto Update", "Desc", 10, 100.00m);
        var createResponse = await _client.PostAsJsonAsync("/api/products", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();

        var updateRequest = new ProductRequest("Produto Atualizado", "Nova desc", 20, 200.00m);
        var response = await _client.PutAsJsonAsync($"/api/products/{created!.Id}", updateRequest);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var product = await response.Content.ReadFromJsonAsync<ProductResponse>();
        product!.Nome.Should().Be("Produto Atualizado");
        product.Valor.Should().Be(200.00m);
        product.DataAtualizacao.Should().NotBeNull();
    }

    [Fact]
    public async Task Update_WithInvalidId_ShouldReturnNotFound()
    {
        var request = new ProductRequest("X", null, 0, 1);
        var response = await _client.PutAsJsonAsync($"/api/products/{Guid.NewGuid()}", request);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_ShouldReturnNoContent()
    {
        var createRequest = new ProductRequest("Produto Delete", "Desc", 10, 50.00m);
        var createResponse = await _client.PostAsJsonAsync("/api/products", createRequest);
        var created = await createResponse.Content.ReadFromJsonAsync<ProductResponse>();

        var response = await _client.DeleteAsync($"/api/products/{created!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await _client.GetAsync($"/api/products/{created.Id}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Delete_WithInvalidId_ShouldReturnNotFound()
    {
        var response = await _client.DeleteAsync($"/api/products/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_WithFilter_ShouldReturnFilteredResults()
    {
        await _client.PostAsJsonAsync("/api/products",
            new ProductRequest("Teclado Especial", "Desc", 5, 299.90m));

        var response = await _client.GetAsync("/api/products?nome=Teclado+Especial");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ProductResponse>>();
        result!.Items.Should().AllSatisfy(p =>
            p.Nome.Should().Contain("Teclado Especial"));
    }

    [Fact]
    public async Task GetAll_WithPagination_ShouldReturnCorrectPage()
    {
        var response = await _client.GetAsync("/api/products?pageNumber=1&pageSize=2");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<PagedResult<ProductResponse>>();
        result!.Items.Count().Should().BeLessThanOrEqualTo(2);
        result.PageNumber.Should().Be(1);
        result.PageSize.Should().Be(2);
    }
}
