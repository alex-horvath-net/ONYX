using FluentAssertions;
using ProductServices.ReadProducts;

namespace Tests.UnitTests.ReadProducts;

public class ValidatorTests {
    [Fact]
    public async Task Yellow_Should_Be_Valid() {
        // Arrange
        var request = new ReadProductsRequest("Yellow");
        var token = CancellationToken.None; 
        
        var fluentValidator = new FluentValidator();
        var validator = new Validator(fluentValidator);

        // Act
        var response = await validator.Validate(request, token);

        // Assert
        response.Should().BeEmpty();
    }

    [Fact]
    public async Task Y_Should_Not_Be_Valid() {
        // Arrange
        var request = new ReadProductsRequest("Y");
        var token = CancellationToken.None;

        var fluentValidator = new FluentValidator();
        var validator = new Validator(fluentValidator);

        // Act
        var response = await validator.Validate(request, token);

        // Assert
        response.Should().NotBeEmpty();
    }
}

