using Common.Dommain;
using Common.Infrastucture.Data;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using ProductServices.CreateProduct;
using Tests.IntegrationTests;

namespace Tests.UnitTests.CreateProduct;

public class RepositoryTests {
    [Fact]
    public async Task CreateProduct_Should_Return_With_New_Product() {
        // Arrange
        var request = new CreateProductRequest(0, "Test Product 1", "Red", 100);
        var token = CancellationToken.None;

        var options = new DbContextOptionsBuilder<AppDB>().UseInMemoryDatabase(Guid.NewGuid().ToString()).Options;
        var db = new AppDB(options);
        db.Seed();

        var repository = new Repository(db);
        var logger = new NullLogger<ResilientRepository>();
        var resilientRepository = new ResilientRepository(repository, logger);
         
        // Act
        var response = await resilientRepository.CreateProduct(request, token);

        // Assert
        response.Should().NotBeNull(); 
    }

   
}

