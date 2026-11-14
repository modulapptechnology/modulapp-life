using Life.Persistence.Auditing;
using Microsoft.EntityFrameworkCore;

namespace Life.API.Tests.Database;

public class EntityAuditingTests
{
    private static readonly DateTime _now = new(2026, 11, 7, 15, 0, 0, DateTimeKind.Utc);

    private static readonly DateTime _createdAt = new(2026, 11, 1, 8, 0, 0, DateTimeKind.Utc);

    private static SampleItem CreateStoredItem()
    {
        return new SampleItem
        {
            Id = Guid.NewGuid(),
            Name = "old",
            CreatedAt = _createdAt,
            UpdatedAt = _createdAt,
        };
    }

    [Fact]
    public void Apply_WhenTheEntityIsAdded_SetsCreationAndModificationDates()
    {
        using var context = SampleDbContext.Create();
        var item = new SampleItem { Name = "new" };
        context.Add(item);

        EntityAuditing.Apply(context.ChangeTracker, _now);

        Assert.Equal((_now, _now), (item.CreatedAt, item.UpdatedAt));
    }

    [Fact]
    public void Add_WhenTheEntityHasNoId_GeneratesAGuid()
    {
        using var context = SampleDbContext.Create();
        var item = new SampleItem { Name = "new" };

        context.Add(item);

        Assert.NotEqual(Guid.Empty, item.Id);
    }

    [Fact]
    public void Apply_WhenTheEntityIsAddedWithAClientId_KeepsThatId()
    {
        using var context = SampleDbContext.Create();
        var clientId = Guid.NewGuid();
        var item = new SampleItem { Id = clientId, Name = "client" };
        context.Add(item);

        EntityAuditing.Apply(context.ChangeTracker, _now);

        Assert.Equal(clientId, item.Id);
    }

    [Fact]
    public void Apply_WhenTheEntityIsModified_UpdatesOnlyTheModificationDate()
    {
        using var context = SampleDbContext.Create();
        var item = CreateStoredItem();
        context.Attach(item);
        item.Name = "changed";

        EntityAuditing.Apply(context.ChangeTracker, _now);

        var createdAtIsModified = context.Entry(item).Property(entity => entity.CreatedAt).IsModified;
        Assert.Equal((_createdAt, _now, false), (item.CreatedAt, item.UpdatedAt, createdAtIsModified));
    }

    [Fact]
    public void Apply_WhenTheEntityIsRemoved_MarksItAsDeletedInsteadOfDeletingIt()
    {
        using var context = SampleDbContext.Create();
        var item = CreateStoredItem();
        context.Attach(item);
        context.Remove(item);

        EntityAuditing.Apply(context.ChangeTracker, _now);

        Assert.Equal(
            (EntityState.Modified, true, (DateTime?)_now),
            (context.Entry(item).State, item.IsDeleted, item.DeletedAt));
    }
}
