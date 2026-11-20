using System.Collections;
using System.Text.Json;
using System.Text.Json.Serialization;
using Life.Application.Sync;
using Life.Domain.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace Life.Persistence.Sync;

public sealed class WorkspaceSyncHandler(LifeDbContext context) : ISyncEntityHandler
{
    public const string WorkspaceEntityType = "workspace";

    public const string ReadOnlyError = "read_only";

    private static readonly JsonSerializerOptions _serializerOptions = BuildSerializerOptions();

    public string EntityType => WorkspaceEntityType;

    public Task<SyncApplyResult> ApplyAsync(
        SyncContext context,
        SyncOperationRequest operation,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(SyncApplyResult.Rejected(ReadOnlyError));
    }

    public async Task<IReadOnlyList<SyncChange>> ListChangesAsync(
        SyncContext syncContext,
        byte[] after,
        int limit,
        CancellationToken cancellationToken = default)
    {
        var rows = await context.Workspaces
            .AsNoTracking()
            .IgnoreQueryFilters()
            .Where(workspace => workspace.Id == syncContext.WorkspaceId
                && StructuralComparisons.StructuralComparer.Compare(workspace.RowVersion, after) > 0)
            .OrderBy(workspace => workspace.RowVersion)
            .Take(limit)
            .ToListAsync(cancellationToken);

        return rows.ConvertAll(workspace => new SyncChange(
            WorkspaceEntityType,
            workspace.Id,
            workspace.IsDeleted,
            workspace.RowVersion,
            workspace.IsDeleted ? null : ToData(workspace)));
    }

    private static JsonElement ToData(Workspace workspace)
    {
        return JsonSerializer.SerializeToElement(
            new WorkspaceSyncData(workspace.Name, workspace.Description, workspace.Type, workspace.Status),
            _serializerOptions);
    }

    private static JsonSerializerOptions BuildSerializerOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        options.Converters.Add(new JsonStringEnumConverter());

        return options;
    }

    private sealed record WorkspaceSyncData(
        string Name,
        string? Description,
        WorkspaceType Type,
        WorkspaceStatus Status);
}
