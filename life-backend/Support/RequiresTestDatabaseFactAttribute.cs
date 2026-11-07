using Xunit;

namespace Support;

public sealed class RequiresTestDatabaseFactAttribute : FactAttribute
{
    public RequiresTestDatabaseFactAttribute()
    {
        if (!TestConfiguration.IsConfigured())
        {
            Skip = $"Missing '{TestDatabaseOptions.ConnectionStringKey}': no test database is configured.";
        }
    }
}
