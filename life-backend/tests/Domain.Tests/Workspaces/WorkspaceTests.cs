using Life.Domain.Common;
using Life.Domain.Workspaces;

namespace Life.Domain.Tests.Workspaces;

public class WorkspaceTests
{
    [Fact]
    public void New_WithoutAssigningValues_IsAnActivePersonalWorkspaceWithoutMembersOrInvitations()
    {
        var workspace = new Workspace();

        var state = (workspace.Type, workspace.Status, workspace.Members.Count, workspace.Invitations.Count);

        Assert.Equal((WorkspaceType.Personal, WorkspaceStatus.Active, 0, 0), state);
    }

    [Fact]
    public void New_WhenAMemberIsCreatedWithoutARole_IsAPlainMember()
    {
        var member = new WorkspaceMember();

        var role = member.Role;

        Assert.Equal(WorkspaceRole.Member, role);
    }

    [Fact]
    public void New_WhenAnInvitationIsCreated_IsPendingAndGrantsThePlainMemberRole()
    {
        var invitation = new WorkspaceInvitation();

        var state = (invitation.Status, invitation.Role);

        Assert.Equal((InvitationStatus.Pending, WorkspaceRole.Member), state);
    }

    [Fact]
    public void Type_WhenDeclared_EveryWorkspaceEntityInheritsFromTheBaseEntity()
    {
        var entityTypes = new[] { typeof(Workspace), typeof(WorkspaceMember), typeof(WorkspaceInvitation) };

        var allInherit = entityTypes.All(entityType => typeof(BaseEntity).IsAssignableFrom(entityType));

        Assert.True(allInherit);
    }
}
