namespace TestSupport.Builders;

public sealed record UsuarioPrueba(Guid Id, string Email, string Nombre, DateTime CreadoEn);

public sealed class UsuarioPruebaBuilder
{
    private static int _contador;

    private Guid _id = Guid.NewGuid();
    private string? _email;
    private string? _nombre;
    private DateTime _creadoEn = new(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    public UsuarioPruebaBuilder ConId(Guid id)
    {
        _id = id;
        return this;
    }

    public UsuarioPruebaBuilder ConEmail(string email)
    {
        _email = email;
        return this;
    }

    public UsuarioPruebaBuilder ConNombre(string nombre)
    {
        _nombre = nombre;
        return this;
    }

    public UsuarioPrueba Build()
    {
        var numero = Interlocked.Increment(ref _contador);
        return new UsuarioPrueba(
            _id,
            _email ?? $"usuario{numero}@life.test",
            _nombre ?? $"Usuario {numero}",
            _creadoEn);
    }
}
