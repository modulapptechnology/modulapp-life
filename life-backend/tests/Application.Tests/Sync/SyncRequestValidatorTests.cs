using System.Text.Json;
using Life.Application.Sync;
using Life.Domain.Sync;

namespace Life.Application.Tests.Sync;

public class SyncRequestValidatorTests
{
    private readonly SyncRequestValidator _validator = new();

    private static SyncOperationRequest NewOperation(
        SyncOperationKind kind = SyncOperationKind.Create,
        JsonElement? payload = null,
        string entityType = "task")
    {
        return new SyncOperationRequest(
            Guid.NewGuid(),
            entityType,
            Guid.NewGuid(),
            kind,
            kind == SyncOperationKind.Delete ? null : payload ?? JsonSerializer.SerializeToElement(new { title = "A" }));
    }

    [Fact]
    public void Validate_WithValidRequest_IsValid()
    {
        var request = new SyncRequest(null, 50, [NewOperation(), NewOperation(SyncOperationKind.Delete)]);

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithOnlyAPull_IsValid()
    {
        var request = new SyncRequest(null, null, []);

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithoutTheOperationList_ReportsTheOperationsField()
    {
        var request = new SyncRequest(null, null, null!);

        var result = _validator.Validate(request);

        Assert.Equal(
            [nameof(SyncRequest.Operations)],
            result.Errors.Select(error => error.PropertyName).Distinct());
    }

    [Fact]
    public void Validate_WithMoreOperationsThanTheLimit_ReportsTheOperationsField()
    {
        var operations = Enumerable.Range(0, SyncRequestValidator.MaxOperations + 1)
            .Select(_ => NewOperation())
            .ToList();
        var request = new SyncRequest(null, null, operations);

        var result = _validator.Validate(request);

        Assert.Equal(
            [nameof(SyncRequest.Operations)],
            result.Errors.Select(error => error.PropertyName).Distinct());
    }

    [Fact]
    public void Validate_WithTheMaximumNumberOfOperations_IsValid()
    {
        var operations = Enumerable.Range(0, SyncRequestValidator.MaxOperations)
            .Select(_ => NewOperation())
            .ToList();
        var request = new SyncRequest(null, null, operations);

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithAnInvalidCursor_ReportsTheCursorField()
    {
        var request = new SyncRequest("not-a-cursor", null, []);

        var result = _validator.Validate(request);

        Assert.Equal(
            [nameof(SyncRequest.Cursor)],
            result.Errors.Select(error => error.PropertyName).Distinct());
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(SyncRequestValidator.MaxPageSize + 1)]
    public void Validate_WithAPageSizeOutOfRange_ReportsThePageSizeField(int pageSize)
    {
        var request = new SyncRequest(null, pageSize, []);

        var result = _validator.Validate(request);

        Assert.Equal(
            [nameof(SyncRequest.PageSize)],
            result.Errors.Select(error => error.PropertyName).Distinct());
    }

    [Fact]
    public void Validate_WithAnEmptyOperationId_ReportsTheOperationIdField()
    {
        var request = new SyncRequest(null, null, [NewOperation() with { OperationId = Guid.Empty }]);

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith(nameof(SyncOperationRequest.OperationId)));
    }

    [Fact]
    public void Validate_WithAnEmptyEntityId_ReportsTheEntityIdField()
    {
        var request = new SyncRequest(null, null, [NewOperation() with { EntityId = Guid.Empty }]);

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith(nameof(SyncOperationRequest.EntityId)));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithABlankEntityType_ReportsTheEntityTypeField(string entityType)
    {
        var request = new SyncRequest(null, null, [NewOperation(entityType: entityType)]);

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith(nameof(SyncOperationRequest.EntityType)));
    }

    [Fact]
    public void Validate_WithACreateWithoutPayload_ReportsThePayloadField()
    {
        var request = new SyncRequest(null, null, [NewOperation() with { Payload = null }]);

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith(nameof(SyncOperationRequest.Payload)));
    }

    [Fact]
    public void Validate_WithAnUnknownOperationKind_ReportsTheKindField()
    {
        var request = new SyncRequest(null, null, [NewOperation() with { Kind = (SyncOperationKind)99 }]);

        var result = _validator.Validate(request);

        Assert.Contains(result.Errors, error => error.PropertyName.EndsWith(nameof(SyncOperationRequest.Kind)));
    }
}
