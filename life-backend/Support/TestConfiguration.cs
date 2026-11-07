using Microsoft.Extensions.Configuration;

namespace Support;

public static class TestConfiguration
{
    public static IConfiguration Build()
    {
        return new ConfigurationBuilder()
            .AddUserSecrets(typeof(TestConfiguration).Assembly, optional: true)
            .AddEnvironmentVariables()
            .Build();
    }

    public static bool IsConfigured()
    {
        return !string.IsNullOrWhiteSpace(Build()[TestDatabaseOptions.ConnectionStringKey]);
    }
}
