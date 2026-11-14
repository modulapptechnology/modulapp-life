using Life.Application.Users;
using Life.Persistence;
using Life.Support;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Life.Flows.Tests.Support;

public sealed class FlowsFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string Project = "Flows";

    private readonly TestDatabase? _database = TestConfiguration.IsConfigured()
        ? new TestDatabase(TestDatabaseOptions.FromConfiguration(TestConfiguration.Build(), Project))
        : null;

    public CapturingEmailSender Emails { get; } = new();

    public string ConnectionString => _database?.ConnectionString ?? string.Empty;

    public LifeDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<LifeDbContext>().UseSqlServer(ConnectionString).Options;

        return new LifeDbContext(options);
    }

    public async Task InitializeAsync()
    {
        if (_database is null)
        {
            return;
        }

        await _database.PrepareEmptyAsync();
        await using var context = CreateContext();
        await context.Database.MigrateAsync();
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();

        if (_database is not null)
        {
            await _database.CleanAsync();
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseContentRoot(FindApiRoot());
        builder.UseSetting("ConnectionStrings:Default", ConnectionString);

        builder.ConfigureTestServices(services =>
        {
            services.AddSingleton<IVerificationEmailSender>(Emails);
        });
    }

    private static string FindApiRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "API", "Life.API.csproj");

            if (File.Exists(candidate))
            {
                return Path.GetDirectoryName(candidate)!;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("The API project folder was not found from the test folder.");
    }
}
