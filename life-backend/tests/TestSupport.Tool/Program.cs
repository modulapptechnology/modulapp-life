using TestSupport;

var command = args.FirstOrDefault()?.ToLowerInvariant();

if (command is not ("prepare" or "clean"))
{
    Console.Error.WriteLine("Usage: dotnet run --project TestSupport.Tool -- <prepare|clean>");
    return 2;
}

try
{
    var options = TestDatabaseOptions.FromConfiguration(TestConfiguration.Build());
    var database = new TestDatabase(options);

    if (command == "prepare")
    {
        await database.PrepareAsync();
        Console.WriteLine($"Database '{options.DatabaseName}' prepared with schema and seed data.");
    }
    else
    {
        await database.CleanAsync();
        Console.WriteLine($"Database '{options.DatabaseName}' dropped.");
    }

    return 0;
}
catch (Exception ex)
{
    Console.Error.WriteLine($"Error: {ex.Message}");
    return 1;
}
