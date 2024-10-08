using System.Net;
using System.Net.Http.Json;
using Common.Dommain;
using FluentAssertions;


namespace Tests.IntegrationTests;
public class ProductsApiIntegrationTests : IClassFixture<ProductApiFactory>
{
    private readonly HttpClient _client;

    public ProductsApiIntegrationTests(ProductApiFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task HealthCheck_ReturnsOk()
    {
        var response = await _client.GetAsync("/api/products/health");
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        content.Should().Be("Service is up and running.");
    }

    [Fact]
    public async Task CreateProduct_ShouldReturnCreatedProduct()
    {
        var product = new { Name = "Test Product 3", Colour = "Blue", Price = 300 };
        var response = await _client.PostAsJsonAsync("/api/products", product);
        response.EnsureSuccessStatusCode();

        var createdProduct = await response.Content.ReadFromJsonAsync<Product>();

        createdProduct.Name.Should().Be("Test Product 3");
    }

    [Theory]
    [InlineData("/api/products", 2)]
    [InlineData("/api/products/colour/red", 1)]
    [InlineData("/api/products/colour/typo", 0)]
    public async Task GetAllProducts_ReturnsOk_WithProductList(string url, int expectedRecords)
    {

        // Act: Send GET request
        var response = await _client.GetAsync(url);

        // Assert: Verify the response is OK
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert: Verify that products are returned in the response
        var products = await response.Content.ReadFromJsonAsync<List<Product>>();
        products.Should().NotBeNull();
        products.Should().HaveCount(expectedRecords);

    }
}
