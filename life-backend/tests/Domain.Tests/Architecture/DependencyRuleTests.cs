using System.Reflection;

namespace Life.Domain.Tests.Architecture;

public class DependencyRuleTests
{
    private static readonly string[] _otherProjects = ["Life.Application", "Life.Infrastructure", "Life.Persistence", "Life.API"];

    [Fact]
    public void Domain_NoReferenciaAOtrosProyectos()
    {
        var ensamblado = Assembly.Load("Life.Domain");

        var referencias = ensamblado
            .GetReferencedAssemblies()
            .Select(referencia => referencia.Name ?? string.Empty)
            .Where(nombre => _otherProjects.Contains(nombre))
            .ToArray();

        Assert.Empty(referencias);
    }
}
