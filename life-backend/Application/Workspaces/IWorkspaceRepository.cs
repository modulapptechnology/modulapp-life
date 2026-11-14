using Life.Domain.Workspaces;

namespace Life.Application.Workspaces;

public interface IWorkspaceRepository
{
    Task<int> CountOwnedAsync(Guid ownerId, CancellationToken cancellationToken = default);

    Task<Workspace?> FindAsync(Guid workspaceId, CancellationToken cancellationToken = default);

    Task<WorkspaceSummaryResponse?> FindByMemberAsync(
        Guid workspaceId,
        Guid userId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<WorkspaceSummaryResponse>> ListByMemberAsync(
        Guid userId,
        CancellationToken cancellationToken = default);

    void Add(Workspace workspace);
}
