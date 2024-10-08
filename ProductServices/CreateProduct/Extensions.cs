using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ProductServices.CreateProduct;

public static class Extensions {
    public static IServiceCollection AddCreateProductService(this IServiceCollection services) {
        services
            .AddScoped<Service>()
            .AddScoped<ICreateProductService>(sp => {
                 var service = sp.GetRequiredService<Service>();
                 var logger = sp.GetRequiredService<ILogger<LoggedService>>();
                 return new LoggedService(service, logger);
             });

        services
            .AddScoped<IValidator, Validator>()
            .AddScoped<IValidator<CreateProductRequest>, FluentValidator>();

        services
            .AddScoped<Repository>()
            .AddScoped<IRepository>(sp => {
                var repository = sp.GetRequiredService<Repository>();
                var logger = sp.GetRequiredService<ILogger<ResilientRepository>>();
                return new ResilientRepository(repository, logger);
            });

        services
            .AddScoped<Publisher>()
            .AddScoped<IPublisher>(sp => {
                var publisher = sp.GetRequiredService<Publisher>();
                var logger = sp.GetRequiredService<ILogger<ResilientPublisher>>();
                return new ResilientPublisher(publisher, logger);
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