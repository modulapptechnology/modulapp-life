using System.Reflection;

namespace Application.Tests.Architecture;

public class DependencyRuleTests
{
    private static readonly string[] ProyectosExternos = ["Infrastructure", "Persistence", "API"];

    [Fact]
    public void Application_NoReferenciaALasCapasExternas()
    {
        var ensamblado = typeof(DependencyInjection).Assembly;

        var referencias = ReferenciasA(ensamblado, ProyectosExternos);

        Assert.Empty(referencias);
    }

    [Fact]
    public void Domain_NoReferenciaAOtrosProyectos()
    {
        var ensamblado = Assembly.Load("Domain");

        var referencias = ReferenciasA(ensamblado, [.. ProyectosExternos, "Application"]);

        Assert.Empty(referencias);
    }

    private static string[] ReferenciasA(Assembly ensamblado, string[] proyectos)
    {
        return ensamblado
            .GetReferencedAssemblies()
            .Select(referencia => referencia.Name ?? string.Empty)
            .Where(nombre => proyectos.Contains(nombre))
            .ToArray();
    }
}
