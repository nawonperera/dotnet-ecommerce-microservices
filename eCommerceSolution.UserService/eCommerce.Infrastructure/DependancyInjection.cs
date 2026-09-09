using eCommerce.Core.RepositoryContracts;
using eCommerce.Infrastructure.DbContext;
using eCommerce.Infrastructure.Repositories;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.Infrastructure;

public static class DependancyInjection
{
    /// <summary>
    /// Extension method to add infrastructure services to the dependancy injection ccontainer
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        //TO DO: Add service to IoC container
        //Infrastructure services often include data access, caching and other low-level components.

        services.AddTransient<IUserRepository, UsersRepository>();
        services.AddTransient<DapperDbContext>();
        return services;
    }
}
