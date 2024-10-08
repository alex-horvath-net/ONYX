using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace ProductServices.ReadProducts;

public static class Extensions {
    public static IServiceCollection AddReadProductsService(this IServiceCollection services) {
        services.AddScoped<IReadProductsService, Service>();

        services
            .AddScoped<IValidator, Validator>()
            .AddScoped<IValidator<ReadProductsRequest>, FluentValidator>();

        services
            .AddScoped<Repository>()
            .AddScoped<IRepository>(sp => {
                var repository = sp.GetRequiredService<Repository>();
                return new ResilientRepository(repository);
        });

        return services;
    }
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