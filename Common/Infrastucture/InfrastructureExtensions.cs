using Azure.Messaging.ServiceBus;
using Common.Infrastucture.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Common.Infrastucture;
public static class InfrastructureExtensions {
    public  static IServiceCollection AddCommonInfrastucture(this IServiceCollection services, ConfigurationManager configuration) {

        var connectionString = configuration.GetConnectionString("AppDB");

        services.AddDbContext<AppDB>(options => options.UseSqlServer(connectionString));

        services.AddSingleton(sp => new ServiceBusClient(configuration.GetValue<string>("ServiceBus:ConnectionString")));

        return services;
    }
} 
