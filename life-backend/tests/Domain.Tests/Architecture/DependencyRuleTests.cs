using System.Reflection;

namespace Domain.Tests.Architecture;

public class DependencyRuleTests
{
    private static readonly string[] OtrosProyectos = ["Application", "Infrastructure", "Persistence", "API"];

    [Fact]
    public void Domain_NoReferenciaAOtrosProyectos()
    {
        var ensamblado = Assembly.Load("Domain");

        var referencias = ensamblado
            .GetReferencedAssemblies()
            .Select(referencia => referencia.Name ?? string.Empty)
            .Where(nombre => OtrosProyectos.Contains(nombre))
            .ToArray();

        Assert.Empty(referencias);
    }
}
