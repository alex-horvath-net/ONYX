using Azure.Messaging.ServiceBus;
using Common.Dommain;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProductServices.CreateProduct;

namespace Tests.UnitTests.CreateProduct;
public class ServiceTests {
    [Fact]
    public async Task Valid_Request_Should_Be_Persisted() {
        // Arrange
        var request = new CreateProductRequest(0, "Test Product 1", "Red", 100);
        var product = new Product { Id = 1, Name = "Test Product 1", Colour = "Red", Price = 100 };
        var token = CancellationToken.None;
        var validator = new Mock<IValidator>();
        validator
            .Setup(v => v.Validate(request, token))
            .ReturnsAsync(new List<string>()); 

        var repository = new Mock<IRepository>();
        repository
            .Setup(r => r.CreateProduct(request, token))
            .ReturnsAsync(product);


        var publisher = new Mock<IPublisher>();
        publisher
            .Setup(c => c.PublishProductCreated(product, token));

        var service = new Service(validator.Object, repository.Object, publisher.Object);
        var loggedService = new LoggedService(service, new NullLogger<LoggedService>());

        // Act
        var response = await loggedService.Execute(request, token);

        // Assert
        response.Issues.Should().BeEmpty();
        response.Product.Should().NotBeNull();
    }

    [Fact]
    public async Task InValid_Request_Should_Not_Be_Persisted() {
        // Arrange
        var request = new CreateProductRequest(0, "T", "Red", 100);
        var product = new Product { Id = 1, Name = "T", Colour = "Red", Price = 100 };
        var token = CancellationToken.None;
        var validator = new Mock<IValidator>();
        validator
            .Setup(v => v.Validate(request, token))
            .ReturnsAsync(new List<string>() { "test issue" });

        var repository = new Mock<IRepository>();
        repository
            .Setup(r => r.CreateProduct(request, token))
            .ReturnsAsync(product);


        var publisher = new Mock<IPublisher>();
        publisher
            .Setup(c => c.PublishProductCreated(product, token));

        var service = new Service(validator.Object, repository.Object, publisher.Object);
        var loggedService = new LoggedService(service, new NullLogger<LoggedService>());

        // Act
        var response = await loggedService.Execute(request, token);

        // Assert
        response.Issues.Should().NotBeEmpty();
        response.Product.Should().BeNull();
    }
}
