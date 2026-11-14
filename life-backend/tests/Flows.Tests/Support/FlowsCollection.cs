namespace Life.Flows.Tests.Support;

[CollectionDefinition(Name)]
public sealed class FlowsCollection : ICollectionFixture<FlowsFactory>
{
    public const string Name = "Flows";
}
