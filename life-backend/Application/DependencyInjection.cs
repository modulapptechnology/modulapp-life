using Application.Users;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddScoped<RegisterUser>();
        services.TryAddScoped<VerifyEmail>();
        services.TryAddScoped<SessionIssuer>();
        services.TryAddScoped<LoginUser>();
        services.TryAddScoped<RefreshSession>();
        services.TryAddScoped<EndSession>();

        return services;
    }
}
