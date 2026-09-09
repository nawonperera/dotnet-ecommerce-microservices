using eCommerce.BusinessLogicLayer.Mappers;
using eCommerce.BusinessLogicLayer.ServiceContracts;
using eCommerce.BusinessLogicLayer.Validators;
using eCommerce.BussinessLogicLayer.RabbitMQ;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using eCommerce.BusinessLogicLayer.Services;

namespace eCommerce.BussinessLogicLayer;

public static class DependencyInjection
{
    public static IServiceCollection AddBussinessLogicLayer(this IServiceCollection services)
    {
        //TO DO: Add Bussiness Logic Layer services into the IoC container.
        services.AddAutoMapper(typeof(ProductAddRequestToProductMappingProfile).Assembly);

        services.AddValidatorsFromAssemblyContaining<ProductAddRequestValidator>();

        services.AddScoped<IProductsService, ProductsService>();

        services.AddTransient<IRabbitMQPublisher, RabbitMQPublisher>();

        return services;
    }
}