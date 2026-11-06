namespace Application.Tests.Architecture;

public class DependencyRuleTests
{
    private static readonly string[] ProyectosExternos = ["Infrastructure", "Persistence", "API"];

    [Fact]
    public void Application_NoReferenciaALasCapasExternas()
    {
        var ensamblado = typeof(DependencyInjection).Assembly;

        var referencias = ensamblado
            .GetReferencedAssemblies()
            .Select(referencia => referencia.Name ?? string.Empty)
            .Where(nombre => ProyectosExternos.Contains(nombre))
            .ToArray();

        Assert.Empty(referencias);
    }
}
