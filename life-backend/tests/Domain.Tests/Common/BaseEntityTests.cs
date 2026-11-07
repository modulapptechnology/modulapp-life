using Domain.Common;

namespace Domain.Tests.Common;

public class BaseEntityTests
{
    private sealed class PlainEntity : BaseEntity
    {
    }

    private sealed class VersionedEntity : BaseEntity, IHasVersionHistory
    {
    }

    [Fact]
    public void Id_WhenAssignedByTheClient_KeepsTheAssignedValue()
    {
        var clientId = Guid.NewGuid();

        var entity = new PlainEntity { Id = clientId };

        Assert.Equal(clientId, entity.Id);
    }

    [Fact]
    public void New_WithoutAssigningValues_IsNotDeletedAndHasNoDeletionDate()
    {
        var entity = new PlainEntity();

        var state = (entity.IsDeleted, entity.DeletedAt);

        Assert.Equal((false, (DateTime?)null), state);
    }

    [Fact]
    public void VersionHistoryMarker_WhenTheEntityDeclaresIt_IdentifiesTheEntity()
    {
        BaseEntity versioned = new VersionedEntity();
        BaseEntity plain = new PlainEntity();

        var identified = (versioned is IHasVersionHistory, plain is IHasVersionHistory);

        Assert.Equal((true, false), identified);
    }
}
