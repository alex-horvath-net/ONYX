using FluentAssertions;
using Moq;
using ProductServices.CreateProduct;

namespace Tests.UnitTests.CreateProduct;

public class ValidatorTests {
    [Fact]
    public async Task Yellow_Should_Be_Valid() {
        // Arrange
        var request = new CreateProductRequest(0, "Test Product 1", "Red", 100);
        var token = CancellationToken.None;

        var repository = new Mock<IRepository>();
        repository
            .Setup(r => r.NameIsUnique(request.Name, token))
            .ReturnsAsync(true);

        var fluentValidator = new FluentValidator(repository.Object);
        var validator = new Validator(fluentValidator);

        // Act
        var response = await validator.Validate(request, token);

        // Assert
        response.Should().BeEmpty();
    }

    [Fact]
    public async Task Y_Should_Not_Be_Valid() {
        // Arrange
        var request = new CreateProductRequest(0, "T", "Red", 100);
        var token = CancellationToken.None;

        var repository = new Mock<IRepository>();
        repository
            .Setup(r => r.NameIsUnique(request.Name, token))
            .ReturnsAsync(true);

        var fluentValidator = new FluentValidator(repository.Object);
        var validator = new Validator(fluentValidator);

        // Act
        var response = await validator.Validate(request, token);

        // Assert
        response.Should().NotBeEmpty();
    }
}

