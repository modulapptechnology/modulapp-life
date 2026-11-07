using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.DependencyInjection;
using TestSupport;

namespace API.Tests.Support;

public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly TestDatabaseFixture? _database = TestConfiguration.IsConfigured() ? new TestDatabaseFixture() : null;

    public string ConnectionString => _database?.ConnectionString ?? string.Empty;

    public HttpClient CreateApiClient(Guid? userId = null)
    {
        var client = CreateClient();

        if (userId is not null)
        {
            client.DefaultRequestHeaders.Add(TestAuthentication.Header, userId.Value.ToString());
        }

        return client;
    }

    public async Task<SqlConnection> OpenConnectionAsync()
    {
        var connection = new SqlConnection(ConnectionString);
        await connection.OpenAsync();
        return connection;
    }

    public Task InitializeAsync()
    {
        return _database?.InitializeAsync() ?? Task.CompletedTask;
    }

    async Task IAsyncLifetime.DisposeAsync()
    {
        await base.DisposeAsync();

        if (_database is not null)
        {
            await _database.DisposeAsync();
        }
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.UseContentRoot(FindApiRoot());

        builder.ConfigureTestServices(services =>
        {
            services
                .AddAuthentication(TestAuthentication.Scheme)
                .AddScheme<AuthenticationSchemeOptions, TestAuthenticationHandler>(TestAuthentication.Scheme, null);

            services.AddSingleton(new TestConnection(ConnectionString));
            services.AddValidatorsFromAssemblyContaining<CreateSampleRequestValidator>();
            services.AddControllers().AddApplicationPart(typeof(TestWorkspacesController).Assembly);
        });
    }

    private static string FindApiRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var candidate = Path.Combine(directory.FullName, "API", "API.csproj");

            if (File.Exists(candidate))
            {
                return Path.GetDirectoryName(candidate)!;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("The API project folder was not found from the test folder.");
    }
}
