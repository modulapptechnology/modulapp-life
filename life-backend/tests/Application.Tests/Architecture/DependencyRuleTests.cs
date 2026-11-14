namespace Life.Application.Tests.Architecture;

public class DependencyRuleTests
{
    private static readonly string[] _externalProjects = ["Life.Infrastructure", "Life.Persistence", "Life.API"];

    [Fact]
    public void Application_NoReferenciaALasCapasExternas()
    {
        var ensamblado = typeof(DependencyInjection).Assembly;

        var referencias = ensamblado
            .GetReferencedAssemblies()
            .Select(referencia => referencia.Name ?? string.Empty)
            .Where(nombre => _externalProjects.Contains(nombre))
            .ToArray();

        Assert.Empty(referencias);
    }
}
