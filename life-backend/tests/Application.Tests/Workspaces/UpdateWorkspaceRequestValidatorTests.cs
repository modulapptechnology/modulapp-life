using Application.Workspaces;

namespace Application.Tests.Workspaces;

public class UpdateWorkspaceRequestValidatorTests
{
    private static readonly byte[] _version = [1, 2, 3, 4, 5, 6, 7, 8];

    private readonly UpdateWorkspaceRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidData_IsValid()
    {
        var request = new UpdateWorkspaceRequest("Home", "Shared tasks", _version);

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithBlankName_FailsOnTheName(string name)
    {
        var request = new UpdateWorkspaceRequest(name, null, _version);

        var result = _validator.Validate(request);

        Assert.Equal(
            [nameof(UpdateWorkspaceRequest.Name)],
            result.Errors.Select(error => error.PropertyName));
    }

    [Fact]
    public void Validate_WithTheMaximumLengths_IsValid()
    {
        var request = new UpdateWorkspaceRequest(
            new string('a', CreateWorkspaceRequestValidator.NameMaxLength),
            new string('b', CreateWorkspaceRequestValidator.DescriptionMaxLength),
            _version);

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithANameOverTheLimit_FailsOnTheName()
    {
        var request = new UpdateWorkspaceRequest(
            new string('a', CreateWorkspaceRequestValidator.NameMaxLength + 1),
            null,
            _version);

        var result = _validator.Validate(request);

        Assert.Equal(
            [nameof(UpdateWorkspaceRequest.Name)],
            result.Errors.Select(error => error.PropertyName));
    }

    [Fact]
    public void Validate_WithADescriptionOverTheLimit_FailsOnTheDescription()
    {
        var request = new UpdateWorkspaceRequest(
            "Home",
            new string('b', CreateWorkspaceRequestValidator.DescriptionMaxLength + 1),
            _version);

        var result = _validator.Validate(request);

        Assert.Equal(
            [nameof(UpdateWorkspaceRequest.Description)],
            result.Errors.Select(error => error.PropertyName));
    }

    [Fact]
    public void Validate_WithoutVersion_FailsOnTheRowVersion()
    {
        var request = new UpdateWorkspaceRequest("Home", null, []);

        var result = _validator.Validate(request);

        Assert.Equal(
            [nameof(UpdateWorkspaceRequest.RowVersion)],
            result.Errors.Select(error => error.PropertyName));
    }
}
