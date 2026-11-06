namespace TestSupport.Builders;

public sealed record EspacioTrabajoPrueba(Guid Id, string Nombre, Guid PropietarioId, DateTime CreadoEn);

public sealed class EspacioTrabajoPruebaBuilder
{
    private static int _contador;

    private readonly Guid _propietarioId;
    private Guid _id = Guid.NewGuid();
    private string? _nombre;

    public EspacioTrabajoPruebaBuilder(Guid propietarioId)
    {
        _propietarioId = propietarioId;
    }

    public EspacioTrabajoPruebaBuilder ConId(Guid id)
    {
        _id = id;
        return this;
    }

    public EspacioTrabajoPruebaBuilder ConNombre(string nombre)
    {
        _nombre = nombre;
        return this;
    }

    public EspacioTrabajoPrueba Build()
    {
        var numero = Interlocked.Increment(ref _contador);
        return new EspacioTrabajoPrueba(
            _id,
            _nombre ?? $"Espacio {numero}",
            _propietarioId,
            new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc));
    }
}
