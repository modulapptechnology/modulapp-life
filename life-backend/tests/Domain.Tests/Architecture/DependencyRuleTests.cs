using System.Reflection;

namespace Domain.Tests.Architecture;

public class DependencyRuleTests
{
    private static readonly string[] _otherProjects = ["Application", "Infrastructure", "Persistence", "API"];

    [Fact]
    public void Domain_NoReferenciaAOtrosProyectos()
    {
        var ensamblado = Assembly.Load("Domain");

        var referencias = ensamblado
            .GetReferencedAssemblies()
            .Select(referencia => referencia.Name ?? string.Empty)
            .Where(nombre => _otherProjects.Contains(nombre))
            .ToArray();

        Assert.Empty(referencias);
    }
}
