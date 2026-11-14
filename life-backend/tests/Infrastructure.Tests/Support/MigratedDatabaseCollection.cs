namespace Life.Infrastructure.Tests.Support;

[CollectionDefinition(Name)]
public sealed class MigratedDatabaseCollection : ICollectionFixture<MigratedDatabaseFixture>
{
    public const string Name = "MigratedDatabase";
}
