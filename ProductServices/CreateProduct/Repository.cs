using Common.Infrastucture.Data;
using Common.Infrastucture.Data.Model;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.CircuitBreaker;
using Polly.Retry;
using DataModel = Common.Infrastucture.Data.Model;
using DomainModel = Common.Dommain;

namespace ProductServices.CreateProduct;
public class Repository(AppDB dB) : IRepository {
    public async Task<DomainModel.Product> CreateProduct(CreateProductRequest request, CancellationToken token) {
        token.ThrowIfCancellationRequested();

        var productDataModel = ToDataModel(request);

        dB.Products.Add(productDataModel);
        await dB.SaveChangesAsync(token);

        var productDomainModel = ToDomainModel(productDataModel);

        return productDomainModel;
    }

    public Task<bool> NameIsUnique(string name, CancellationToken token) {
        token.ThrowIfCancellationRequested();

        return dB
            .Products
            .AsNoTracking()
            .AnyAsync(x => x.Name == name, token)
            .ContinueWith(x => !x.Result);
    }

    private static DomainModel.Product ToDomainModel(Product productDataModel) => new() {
        Id = productDataModel.Id,
        Name = productDataModel.Name,
        Colour = productDataModel.Colour,
        Price = productDataModel.Price
    };

    private static Product ToDataModel(CreateProductRequest request) => new() {
        Name = request.Name,
        Colour = request.Colour,
        Price = request.Price
    };
}

public class ResilientRepository : IRepository {

    private readonly AsyncRetryPolicy retryPolicy;
    private readonly AsyncCircuitBreakerPolicy circuitBreakerPolicy;
    private readonly IRepository repository;

    public ResilientRepository(IRepository repository, ILogger<ResilientRepository> logger) {
        this.repository = repository;
        // Define the retry policy: 3 retries, with exponential backoff starting from 1 second
        retryPolicy = Policy
            .Handle<SqlException>()                      // SQL-related exceptions
            .Or<TimeoutException>()                      // Operation timeout exceptions
            .Or<HttpRequestException>()                  // In case HTTP requests are involved (for microservices)
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

        // Define the circuit breaker policy: break for 30 seconds after 2 consecutive exceptions
        circuitBreakerPolicy = Policy
            .Handle<SqlException>()
            .Or<TimeoutException>()
            .Or<HttpRequestException>()
            .CircuitBreakerAsync(2, TimeSpan.FromSeconds(30));
    }

    public Task<DomainModel.Product> CreateProduct(CreateProductRequest request, CancellationToken token) =>
        retryPolicy.ExecuteAsync(() =>
        circuitBreakerPolicy.ExecuteAsync(() =>
        repository.CreateProduct(request, token)));

    public Task<bool> NameIsUnique(string name, CancellationToken token) =>
       retryPolicy.ExecuteAsync(() =>
       circuitBreakerPolicy.ExecuteAsync(() =>
       repository.NameIsUnique(name, token)));
}
