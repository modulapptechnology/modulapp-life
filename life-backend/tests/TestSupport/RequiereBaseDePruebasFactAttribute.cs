using Xunit;

namespace TestSupport;

public sealed class RequiereBaseDePruebasFactAttribute : FactAttribute
{
    public RequiereBaseDePruebasFactAttribute()
    {
        if (!TestConfiguration.IsConfigured())
        {
            Skip = $"Falta '{TestDatabaseOptions.ConnectionStringKey}': no hay base de pruebas configurada.";
        }
    }
}
