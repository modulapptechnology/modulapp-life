using Support;

var command = args.ElementAtOrDefault(0)?.ToLowerInvariant();
var project = args.ElementAtOrDefault(1);

if (command is not ("prepare" or "clean") || string.IsNullOrWhiteSpace(project))
{
    Console.Error.WriteLine("Usage: dotnet run --project tests/Support.Tool -- <prepare|clean> <Project>  (for example: prepare Api)");
    return 2;
}

try
{
    var options = TestDatabaseOptions.FromConfiguration(TestConfiguration.Build(), project);
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
