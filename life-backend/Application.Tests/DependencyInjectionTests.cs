using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Application.Tests;

public class DependencyInjectionTests
{
    [Fact]
    public void AddApplication_ConColeccionValida_DevuelveLaMismaColeccion()
    {
        var services = new ServiceCollection();

        var resultado = services.AddApplication();

        Assert.Same(services, resultado);
    }

    [Fact]
    public void AddApplication_ConColeccionSimulada_DevuelveLaMismaInstancia()
    {
        var services = Substitute.For<IServiceCollection>();

        var resultado = services.AddApplication();

        Assert.Same(services, resultado);
    }

    [Fact]
    public void AddApplication_LlamadoDosVeces_NoDuplicaLosRegistros()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        var registrosIniciales = services.Count;

        services.AddApplication();

        Assert.Equal(registrosIniciales, services.Count);
    }
}
