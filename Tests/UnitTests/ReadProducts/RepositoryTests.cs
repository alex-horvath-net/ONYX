using Common.Dommain;
using Common.Infrastucture.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Moq;
using ProductServices.ReadProducts;
using Tests.IntegrationTests;

namespace Tests.UnitTests.ReadProducts;

public class RepositoryTests {
    [Fact]
    public async Task ReadProducts_Should_Return_All_Products_When_Colour_Is_Null() {
        // Arrange
        var request = new ReadProductsRequest(null);
        var token = CancellationToken.None;

        var options = new DbContextOptionsBuilder<AppDB>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var db = new AppDB(options);
        db.Seed();

        var repository = new Repository(db);
        var resilientRepository = new ResilientRepository(repository);

        // Act
        var response = await resilientRepository.ReadProducts(request, token);

        // Assert
        response.Should().HaveCount(2);
    }

    [Fact]
    public async Task ReadProducts_Should_Return_Red_Products_When_Colour_Is_Red() {
        // Arrange
        var request = new ReadProductsRequest("Red");
        var token = CancellationToken.None;

        var options = new DbContextOptionsBuilder<AppDB>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var db = new AppDB(options);
        db.Seed();

        var repository = new Repository(db);
        var resilientRepository = new ResilientRepository(repository);

        // Act
        var response = await resilientRepository.ReadProducts(request, token);

        // Assert
        response.Should().HaveCount(1);
    }
}

