using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Servicios externos: correo, archivos, integraciones (Transient o Singleton según el caso).
        return services;
    }
}
