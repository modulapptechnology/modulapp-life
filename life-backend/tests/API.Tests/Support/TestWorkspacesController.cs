using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using API.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;

namespace API.Tests.Support;

public sealed record TestConnection(string ConnectionString);

public sealed record WorkspaceResponse(Guid Id, string Name);

public sealed class UpdateWorkspaceRequest
{
    [Required]
    [MaxLength(100)]
    public string Name { get; init; } = string.Empty;
}

[Authorize]
public class TestWorkspacesController(TestConnection connection) : ApiControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Get(Guid id)
    {
        var workspace = await FindAsync(id);

        if (workspace is null)
        {
            return NotFound();
        }

        if (workspace.Value.OwnerId != CurrentUserId())
        {
            return Forbid();
        }

        return Ok(new WorkspaceResponse(id, workspace.Value.Name));
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateWorkspaceRequest request)
    {
        var workspace = await FindAsync(id);

        if (workspace is null)
        {
            return NotFound();
        }

        if (workspace.Value.OwnerId != CurrentUserId())
        {
            return Forbid();
        }

        await using var sqlConnection = new SqlConnection(connection.ConnectionString);
        await sqlConnection.OpenAsync();
        await using var command = new SqlCommand(
            "UPDATE dbo.Workspaces SET Name = @Name WHERE Id = @Id",
            sqlConnection);
        command.Parameters.AddWithValue("@Name", request.Name);
        command.Parameters.AddWithValue("@Id", id);
        await command.ExecuteNonQueryAsync();

        return NoContent();
    }

    private Guid CurrentUserId()
    {
        return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
    }

    private async Task<(Guid OwnerId, string Name)?> FindAsync(Guid id)
    {
        await using var sqlConnection = new SqlConnection(connection.ConnectionString);
        await sqlConnection.OpenAsync();
        await using var command = new SqlCommand(
            "SELECT OwnerId, Name FROM dbo.Workspaces WHERE Id = @Id",
            sqlConnection);
        command.Parameters.AddWithValue("@Id", id);
        await using var reader = await command.ExecuteReaderAsync();

        return await reader.ReadAsync() ? (reader.GetGuid(0), reader.GetString(1)) : null;
    }
}
