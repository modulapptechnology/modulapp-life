using TestSupport.Builders;
using TestSupport.Seed;

namespace TestSupport.Tests;

public class BuildersTests
{
    [Fact]
    public void Build_SinValoresFijados_GeneraUsuariosConEmailUnico()
    {
        var builder = new UsuarioPruebaBuilder();

        var emails = new[] { builder.Build().Email, builder.Build().Email };

        Assert.Equal(2, emails.Distinct().Count());
    }

    [Fact]
    public void Build_ConValoresFijados_ConservaLosValores()
    {
        var builder = new UsuarioPruebaBuilder().ConEmail("a@life.test").ConNombre("Ana");

        var usuario = builder.Build();

        Assert.Equal(("a@life.test", "Ana"), (usuario.Email, usuario.Nombre));
    }

    [Fact]
    public void Build_ConPropietario_AsignaElPropietarioAlEspacio()
    {
        var propietario = new UsuarioPruebaBuilder().Build();
        var builder = new EspacioTrabajoPruebaBuilder(propietario.Id);

        var espacio = builder.Build();

        Assert.Equal(propietario.Id, espacio.PropietarioId);
    }

    [Fact]
    public void DatosSemilla_Espacios_TienenComoPropietarioUnUsuarioSemilla()
    {
        var idsDeUsuarios = DatosSemilla.Usuarios.Select(usuario => usuario.Id);

        var propietarios = DatosSemilla.Espacios.Select(espacio => espacio.PropietarioId);

        Assert.All(propietarios, propietario => Assert.Contains(propietario, idsDeUsuarios));
    }

    [Fact]
    public void DatosSemilla_Usuarios_TienenEmailsDistintos()
    {
        var usuarios = DatosSemilla.Usuarios;

        var emails = usuarios.Select(usuario => usuario.Email).Distinct();

        Assert.Equal(usuarios.Count, emails.Count());
    }
}
