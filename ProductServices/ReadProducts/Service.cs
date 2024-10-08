using Common.Dommain;
using Microsoft.Extensions.Logging;
using ProductServices.CreateProduct;

namespace ProductServices.ReadProducts;

public class Service(IValidator validator, IRepository repository) : IReadProductsService {
    public async Task<ReadProductsResponse> Execute(ReadProductsRequest request, CancellationToken token) {
        token.ThrowIfCancellationRequested();
        var response = new ReadProductsResponse();

        response.Issues = await validator.Validate(request, token);
        if (response.Issues.Any()) {
            return response;
        }

        response.Products = await repository.ReadProducts(request, token);

        return response;
    }
}

public class LoggedService(IReadProductsService service, ILogger<LoggedService> logger) : IReadProductsService {
    public async Task<ReadProductsResponse> Execute(ReadProductsRequest request, CancellationToken token) {

        ReadProductsResponse response = null;
        try {
            logger.LogDebug("Creating product {Colour}", request.Colour);

            response = await service.Execute(request, token);

            logger.LogInformation("Product {Colour} created", request.Colour);
        } catch (Exception ex) {
            logger.LogError(ex, "Error is raised during reaed products {Colour}", request.Colour);
            throw;
        }
        return response;
    }
}

public record ReadProductsRequest(string? Colour);

public class ReadProductsResponse {
    public List<string> Issues { get; set; } = [];
    public List<Product> Products { get; set; } = [];

}
public interface IReadProductsService {
    public Task<ReadProductsResponse> Execute(ReadProductsRequest request, CancellationToken token);
}

public interface IValidator {
    public Task<List<string>> Validate(ReadProductsRequest request, CancellationToken token);
}
public interface IRepository {
    Task<List<Product>> ReadProducts(ReadProductsRequest request, CancellationToken token);
}

