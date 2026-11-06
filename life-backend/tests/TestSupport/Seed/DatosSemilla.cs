using TestSupport.Builders;

namespace TestSupport.Seed;

public static class DatosSemilla
{
    public static readonly UsuarioPrueba Propietario = new UsuarioPruebaBuilder()
        .ConId(new Guid("00000000-0000-0000-0000-000000000001"))
        .ConEmail("propietario@life.test")
        .ConNombre("Propietario de prueba")
        .Build();

    public static readonly UsuarioPrueba Miembro = new UsuarioPruebaBuilder()
        .ConId(new Guid("00000000-0000-0000-0000-000000000002"))
        .ConEmail("miembro@life.test")
        .ConNombre("Miembro de prueba")
        .Build();

    public static readonly EspacioTrabajoPrueba EspacioPrincipal = new EspacioTrabajoPruebaBuilder(Propietario.Id)
        .ConId(new Guid("00000000-0000-0000-0000-0000000000a1"))
        .ConNombre("Espacio principal de prueba")
        .Build();

    public static IReadOnlyList<UsuarioPrueba> Usuarios { get; } = [Propietario, Miembro];

    public static IReadOnlyList<EspacioTrabajoPrueba> Espacios { get; } = [EspacioPrincipal];
}
