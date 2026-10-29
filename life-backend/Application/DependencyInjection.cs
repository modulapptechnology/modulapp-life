using Microsoft.Extensions.DependencyInjection;

namespace Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Casos de uso (Scoped). Sin servicios de negocio por ahora.
        return services;
    }
}
