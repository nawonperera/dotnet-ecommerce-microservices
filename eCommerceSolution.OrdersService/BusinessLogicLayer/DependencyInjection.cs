using eCommerce.OrderMicroservice.BusinessLogicLayer.Validators;
using eCommerce.OrderMicroservice.BusinessLogicLayer.Services;
using eCommerce.OrderMicroservice.BusinessLogicLayer.Mappers;
using eCommerce.OrderMicroservice.BusinessLogicLayer.ServiceContracts;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using eCommerce.OrderMicroservice.BussinessLogicLayer.RabbitMQ;

namespace eCommerce.OrderMicroservice.BusinessLogicLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddBusinessLogicLayer(this IServiceCollection services, IConfiguration configuration)
    {
        //Add data access layer services into the IoC container
        services.AddValidatorsFromAssemblyContaining<OrderAddRequestValidator>();

        services.AddAutoMapper(typeof(OrderAddRequestToOrderMappingProfile).Assembly);

        services.AddScoped<IOrdersService, OrdersService>();

        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = $"{configuration["REDIS_HOST"]}:{configuration["REDIS_PORT"]}";
        });

        services.AddTransient<IRabbitMQProductNameUpdateConsumer, RabbitMQProductNameUpdateConsumer>();
        services.AddTransient<IRabbitMQProductDeletionConsumer, RabbitMQProductDeletionConsumer>();

        services.AddHostedService<RabbitMQProductDeletionHostedService>();

        return services;
    }
}
