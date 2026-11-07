using Microsoft.Data.SqlClient;
using Support;
using Support.Builders;

namespace API.Tests.Support;

public sealed class TemporaryWorkspace : IAsyncDisposable
{
    private readonly ApiFactory _api;

    private TemporaryWorkspace(ApiFactory api, TestWorkspace workspace)
    {
        _api = api;
        Workspace = workspace;
    }

    public TestWorkspace Workspace { get; }

    public static async Task<TemporaryWorkspace> CreateAsync(ApiFactory api, Guid ownerId)
    {
        var workspace = new TestWorkspaceBuilder(ownerId).Build();

        await using var connection = await api.OpenConnectionAsync();
        await TestDatabase.InsertAsync(connection, workspace);

        return new TemporaryWorkspace(api, workspace);
    }

    public async Task<string?> ReadNameAsync()
    {
        await using var connection = await _api.OpenConnectionAsync();
        await using var command = new SqlCommand("SELECT Name FROM dbo.Workspaces WHERE Id = @Id", connection);
        command.Parameters.AddWithValue("@Id", Workspace.Id);

        return await command.ExecuteScalarAsync() as string;
    }

    public async ValueTask DisposeAsync()
    {
        await using var connection = await _api.OpenConnectionAsync();
        await using var command = new SqlCommand("DELETE FROM dbo.Workspaces WHERE Id = @Id", connection);
        command.Parameters.AddWithValue("@Id", Workspace.Id);
        await command.ExecuteNonQueryAsync();
    }
}
