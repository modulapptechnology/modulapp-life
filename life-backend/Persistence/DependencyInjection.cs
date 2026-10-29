using Microsoft.Extensions.DependencyInjection;

namespace Persistence;

public static class DependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services)
    {
        // DbContext y repositorios (Scoped). El DbContext se agrega con la tarea de SQL Server.
        return services;
    }
}
