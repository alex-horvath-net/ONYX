using Common.Dommain;
using Microsoft.Extensions.Logging;

namespace ProductServices.CreateProduct;

public class Service(IValidator validator, IRepository repository, IPublisher publisher) : ICreateProductService {
    public async Task<CreateProductResponse> Execute(CreateProductRequest request, CancellationToken token) {
        // This method responsibility to manage the business workflow, but it is 100% technology agnostic.
        // It does not know how the data is stored, how the data is validated, or how the data is published.

        token.ThrowIfCancellationRequested();

        var response = new CreateProductResponse();

        response.Issues = await validator.Validate(request, token);
        if (response.Issues.Any()) {
            return response;
        }

        response.Product = await repository.CreateProduct(request, token);

        await publisher.PublishProductCreated(response.Product, token);

        return response;
    }
}

public class LoggedService(ICreateProductService service, ILogger<LoggedService> logger) : ICreateProductService {
    public async Task<CreateProductResponse> Execute(CreateProductRequest request, CancellationToken token) {

        CreateProductResponse response = null;
        try {
            logger.LogDebug("Creating product {Name}", request.Name);

            response = await service.Execute(request, token);

            logger.LogInformation("Product {Name} created", request.Name);
        } catch (Exception ex) {
            logger.LogError(ex, "Error is raised during creating new product {Name}", request.Name);
            throw;
        }
        return response;
    }
}


public record CreateProductRequest(int Id, string Name, string Colour, decimal Price);

public record CreateProductResponse {
    public List<string> Issues { get; set; } = [];
    public Product Product { get; set; }
}

public interface ICreateProductService {
    public Task<CreateProductResponse> Execute(CreateProductRequest request, CancellationToken token);
}

public interface IValidator {
    public Task<List<string>> Validate(CreateProductRequest request, CancellationToken token);
}
public interface IRepository {
    public Task<Product> CreateProduct(CreateProductRequest request, CancellationToken token);
    public Task<bool> NameIsUnique(string name, CancellationToken token);
}

public interface IPublisher {
    public Task PublishProductCreated(Product product, CancellationToken token);
}
// Product Service creates a Product
// Product Service publishes a ProductCreated
//      Product Service accept validation requests from Order Service

// Order Service listess for ProductCreated
//      Order Service calls the Product Service to validate the details
// Order Service creates Order with status as Pending
// Order Service publishes a OrderPlaced 

// Payment Service listess for OrderPlaced
// Payment Service processes the payment
// Payment Service publishes a PaymentProcessed  

// Order Service listess for PaymentProcessed 
// Order Service updates the order status to completed 