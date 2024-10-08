using FluentValidation;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ProductServices.ReadProducts;

public static class Extensions {
    public static IServiceCollection AddReadProductsService(this IServiceCollection services) {
        services.AddScoped<IReadProductsService, Service>();

        services
            .AddScoped<IValidator, Validator>()
            .AddScoped<IValidator<ReadProductsRequest>, FluentValidator>();

        services
            .AddDistributedMemoryCache()
            .AddScoped<Repository>()
            .AddScoped<IRepository>(sp => {
                var repository = sp.GetRequiredService<Repository>();
                var cache = sp.GetRequiredService<IDistributedCache>();
                var cachedRepository = new CachedRepository(repository, cache);
                var resilientRepository  = new ResilientRepository(repository);
                return resilientRepository;
            });

        return services;
    }
}
