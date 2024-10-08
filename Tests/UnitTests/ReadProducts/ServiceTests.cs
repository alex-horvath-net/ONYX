using Common.Dommain;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProductServices.ReadProducts;

namespace Tests.UnitTests.ReadProducts;
public class ServiceTests {
    [Fact]
    public async Task Existing_Product_should_be_found() { 
        // Arrange
        var request = new ReadProductsRequest("Red");
        var token = CancellationToken.None;
        var validator = new Mock<IValidator>();
        validator
            .Setup(v => v.Validate(request, token))
            .ReturnsAsync(new List<string>());

        var repository = new Mock<IRepository>();
        repository
            .Setup(r => r.ReadProducts(request, token))
            .ReturnsAsync(new List<Product> { new() { Name = "Test Product 1", Colour = "Red", Price = 100 } });

        var service = new Service(validator.Object, repository.Object);
        var loggedService = new LoggedService(service, new NullLogger<LoggedService>());

        // Act
        var response = await loggedService.Execute(request, token);

        // Assert
        response.Issues.Should().BeEmpty();
        response.Products.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Non_Existing_Product_Should_Be_Not_Found() {
        // Arrange
        var request = new ReadProductsRequest("r");
        var token = CancellationToken.None;
        var validator = new Mock<IValidator>();
        validator
            .Setup(v => v.Validate(request, token))
            .ReturnsAsync(new List<string>() { "Colour must be at least 3 character long, if it is provided." });

        var repository = new Mock<IRepository>();
        repository
            .Setup(r => r.ReadProducts(request, token))
            .ReturnsAsync(new List<Product> { new() { Name = "Test Product 1", Colour = "Red", Price = 100 } });

        var service = new Service(validator.Object, repository.Object);
        var loggedService = new LoggedService(service, new NullLogger<LoggedService>());

        // Act
        var response = await loggedService.Execute(request, token);

        // Assert
        response.Issues.Should().NotBeEmpty();
        response.Products.Should().BeEmpty();
    }
}
