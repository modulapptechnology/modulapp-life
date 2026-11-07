using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddInfrastructure_WithValidCollection_ReturnsSameCollection()
    {
        var services = new ServiceCollection();

        var result = services.AddInfrastructure();

        Assert.Same(services, result);
    }
}
