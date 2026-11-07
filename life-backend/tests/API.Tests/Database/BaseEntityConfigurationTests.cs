using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Persistence.Conversion;

namespace API.Tests.Database;

public class BaseEntityConfigurationTests
{
    private static IEntityType GetItemEntityType(SampleDbContext context)
    {
        return context.Model.FindEntityType(typeof(SampleItem))!;
    }

    [Fact]
    public void Model_WhenAnEntityInheritsTheBase_UsesTheIdAsGuidKeyGeneratedOnAdd()
    {
        using var context = SampleDbContext.Create();

        var key = GetItemEntityType(context).FindPrimaryKey()!.Properties.Single();

        Assert.Equal(
            ("Id", typeof(Guid), ValueGenerated.OnAdd),
            (key.Name, key.ClrType, key.ValueGenerated));
    }

    [Fact]
    public void Model_WhenAnEntityInheritsTheBase_RequiresTheAuditFields()
    {
        using var context = SampleDbContext.Create();

        var entityType = GetItemEntityType(context);

        Assert.All(
            new[] { "CreatedAt", "UpdatedAt", "IsDeleted" },
            name => Assert.False(entityType.FindProperty(name)!.IsNullable));
        Assert.True(entityType.FindProperty("DeletedAt")!.IsNullable);
    }

    [Fact]
    public void Model_WhenAnEntityInheritsTheBase_UsesTheRowVersionAsConcurrencyToken()
    {
        using var context = SampleDbContext.Create();

        var rowVersion = GetItemEntityType(context).FindProperty("RowVersion")!;

        Assert.Equal(
            (true, ValueGenerated.OnAddOrUpdate, "rowversion"),
            (rowVersion.IsConcurrencyToken, rowVersion.ValueGenerated, rowVersion.GetColumnType()));
    }

    [Fact]
    public void Model_WhenAnEntityInheritsTheBase_StoresEveryDateAsUtc()
    {
        using var context = SampleDbContext.Create();

        var entityType = GetItemEntityType(context);

        Assert.All(
            new[] { "CreatedAt", "UpdatedAt", "DeletedAt" },
            name => Assert.IsType<UtcDateTimeConverter>(entityType.FindProperty(name)!.GetValueConverter()));
    }

    [Fact]
    public void Query_ByDefault_ExcludesTheLogicallyDeletedRecords()
    {
        using var context = SampleDbContext.Create();

        var sql = context.Items.ToQueryString();

        Assert.Contains("[IsDeleted]", sql);
    }

    [Fact]
    public void Query_IgnoringTheQueryFilters_DoesNotExcludeTheLogicallyDeletedRecords()
    {
        using var context = SampleDbContext.Create();

        var sql = context.Items.IgnoreQueryFilters().ToQueryString();

        Assert.DoesNotContain("WHERE", sql);
    }

    [Fact]
    public void UtcDateTimeConverter_WhenWritingALocalDate_ConvertsItToUtc()
    {
        var converter = new UtcDateTimeConverter();
        var local = new DateTime(2026, 11, 7, 10, 0, 0, DateTimeKind.Local);

        var stored = (DateTime)converter.ConvertToProvider(local)!;

        Assert.Equal((local.ToUniversalTime(), DateTimeKind.Utc), (stored, stored.Kind));
    }

    [Fact]
    public void UtcDateTimeConverter_WhenReadingAStoredDate_MarksItAsUtc()
    {
        var converter = new UtcDateTimeConverter();
        var stored = new DateTime(2026, 11, 7, 15, 0, 0, DateTimeKind.Unspecified);

        var read = (DateTime)converter.ConvertFromProvider(stored)!;

        Assert.Equal((stored, DateTimeKind.Utc), (read, read.Kind));
    }
}
