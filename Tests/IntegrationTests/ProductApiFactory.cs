using Common.Infrastucture.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ProductsApi.Controllers;
using ProductServices.CreateProduct;


namespace Tests.IntegrationTests;

public class ProductApiFactory : WebApplicationFactory<ProductsController>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(services => {
            UseFakeAuthentication(services);
            UseFakePublisher(services);
            UseInMemoryDB(services);
            SeedDB(services);
        });
    }

    private void UseFakeAuthentication(IServiceCollection services) {
        // Replace the authentication scheme with a fake one for testing
        services.AddAuthentication("FakeBearer")
                .AddScheme<AuthenticationSchemeOptions, FakeJwtBearerAuthenticationHandler>("FakeBearer", options => { });

        // Ensure the authorization service uses the fake scheme
        services.Configure<AuthenticationOptions>(options =>
        {
            options.DefaultAuthenticateScheme = "FakeBearer";
            options.DefaultChallengeScheme = "FakeBearer";
        });
    }

    private void SeedDB(IServiceCollection services) {
        var serviceProvider = services.BuildServiceProvider();
        using (var scope = serviceProvider.CreateScope()) {
            var db = scope.ServiceProvider.GetRequiredService<AppDB>();
            db.Database.EnsureCreated();
            db.Seed();
        }
    }

    private void UseFakePublisher(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(IPublisher));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        services.AddSingleton<IPublisher, FakePublisher>();
    }

    private static void UseInMemoryDB(IServiceCollection services)
    {
        var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDB>));
        if (descriptor != null)
        {
            services.Remove(descriptor);
        }

        services.AddDbContext<AppDB>(options =>
        {
            options.UseInMemoryDatabase("InMemoryAppDB");
        });
    }

}
