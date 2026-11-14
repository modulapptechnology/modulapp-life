using FluentValidation;
using Life.Application.Users;
using Life.Application.Workspaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Life.Application;

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
        services.TryAddScoped<CreateWorkspace>();
        services.TryAddScoped<ListWorkspaces>();
        services.TryAddScoped<GetWorkspace>();
        services.TryAddScoped<UpdateWorkspace>();
        services.TryAddScoped<ArchiveWorkspace>();

        return services;
    }
}
