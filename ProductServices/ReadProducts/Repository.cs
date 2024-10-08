using Common.Dommain;
using Common.Infrastucture.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;

namespace ProductServices.ReadProducts;
public class Repository(AppDB dB) : IRepository {
    public async Task<List<Product>> ReadProducts(ReadProductsRequest request, CancellationToken token) {
        token.ThrowIfCancellationRequested();

        var productDataModelList =
            request.Colour == null ?
            await dB.Products.AsNoTracking().ToListAsync(token) :
            await dB.Products.AsNoTracking().Where(x => x.Colour.ToLower() == request.Colour.ToLower()).ToListAsync(token);

        var products = productDataModelList.Select(ToDomainModel).ToList();

        return products;
    }

    private static Product ToDomainModel(Common.Infrastucture.Data.Model.Product dataModel) => new () {
        Id = dataModel.Id,
        Name = dataModel.Name,
        Colour = dataModel.Colour,
        Price = dataModel.Price
    };
}

public class ResilientRepository : IRepository {

    private readonly AsyncRetryPolicy retryPolicy;
    private readonly AsyncCircuitBreakerPolicy circuitBreakerPolicy;
    private readonly IRepository repository;

    public ResilientRepository(IRepository repository) {
        this.repository = repository;
        // Define the retry policy: 3 retries, with exponential backoff starting from 1 second
        retryPolicy = Policy
            .Handle<DbUpdateException>()                 // Entity Framework specific exceptions
            .Or<SqlException>()                          // SQL-related exceptions
            .Or<TimeoutException>()                      // Operation timeout exceptions
            .Or<TaskCanceledException>()                 // Task cancellations (useful for async operations)
            .Or<HttpRequestException>()                  // In case HTTP requests are involved (for microservices)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        // Define the circuit breaker policy: break for 30 seconds after 2 consecutive exceptions
        circuitBreakerPolicy = Policy
            .Handle<DbUpdateException>()
            .Or<SqlException>()
            .Or<TimeoutException>()
            .Or<TaskCanceledException>()
            .Or<HttpRequestException>()
            .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30));
    }

    public Task<List<Product>> ReadProducts(ReadProductsRequest request, CancellationToken token) {
        return retryPolicy.ExecuteAsync(() => {
            return circuitBreakerPolicy.ExecuteAsync(() => {
                return repository.ReadProducts(request, token);
            });
        });
    }
}
