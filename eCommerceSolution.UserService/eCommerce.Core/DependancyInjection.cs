using eCommerce.Core.ServiceContracts;
using eCommerce.Core.Services;
using eCommerce.Core.Validators;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace eCommerce.Core;

public static class DependancyInjection
{
    /// <summary>
    /// Extension method to add Core services to the dependancy injection container
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection AddCore(this IServiceCollection services)
    {
        //TO DO: Add service to IoC container
        //Core services often include data access, caching and other low-level components.

        services.AddTransient<IUsersService,UsersService>();
        services.AddValidatorsFromAssemblyContaining<LoginRequestValidator>();
        return services;
    }
}
