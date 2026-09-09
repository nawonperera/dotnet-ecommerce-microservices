using eCommerce.OrderMicroservice.DataAccessLayer.Repositories;
using eCommerce.OrderMicroservice.DataAccessLayer.RepositoryContracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MongoDB.Driver;

namespace eCommerce.OrderMicroservice.DataAccessLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddDataAccessLayer(this IServiceCollection services, IConfiguration configuration)
    {
        //Add data access layer services into the IoC container
        string connectionStringTemplate = configuration.GetConnectionString("MongoDB")!;
        string connectionString = connectionStringTemplate
            .Replace("$MONGODB_HOST", Environment.GetEnvironmentVariable("MONGODB_HOST") ?? "localhost")
            .Replace("$MONGODB_PORT", Environment.GetEnvironmentVariable("MONGODB_PORT") ?? "27017");

        // Register MongoClient as Singleton (ONE instance for entire app)
        // This is correct → MongoClient is thread-safe and expensive to create
        services.AddSingleton<IMongoClient>(new MongoClient(connectionString));

        // Register IMongoDatabase as Scoped (new per request)
        // This is fine because it's lightweight wrapper around MongoClient
        services.AddScoped<IMongoDatabase>(provider =>
        {
            // Get the already-created MongoClient from DI container
            IMongoClient client = provider.GetRequiredService<IMongoClient>();

            // Get database instance (this does NOT create a DB, just references it)
            return client.GetDatabase(Environment.GetEnvironmentVariable("MONGODB_DATABASE"));
        });

        services.AddScoped<IOrdersRepository, OrdersRepository>();

        return services;
    }
}
