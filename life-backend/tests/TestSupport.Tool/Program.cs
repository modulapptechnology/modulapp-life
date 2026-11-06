using TestSupport;

var comando = args.FirstOrDefault()?.ToLowerInvariant();

if (comando is not ("prepare" or "clean"))
{
    Console.Error.WriteLine("Uso: dotnet run --project TestSupport.Tool -- <prepare|clean>");
    return 2;
}

try
{
    var opciones = TestDatabaseOptions.FromConfiguration(TestConfiguration.Build());
    var database = new TestDatabase(opciones);

    if (comando == "prepare")
    {
        await database.PrepareAsync();
        Console.WriteLine($"Base '{opciones.DatabaseName}' preparada con esquema y datos semilla.");
    }
    else
    {
        await database.CleanAsync();
        Console.WriteLine($"Base '{opciones.DatabaseName}' eliminada.");
    }

    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}
