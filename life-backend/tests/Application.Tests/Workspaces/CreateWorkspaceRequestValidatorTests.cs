using Application.Workspaces;
using Domain.Workspaces;

namespace Application.Tests.Workspaces;

public class CreateWorkspaceRequestValidatorTests
{
    private readonly CreateWorkspaceRequestValidator _validator = new();

    [Fact]
    public void Validate_WithValidRequest_IsValid()
    {
        var request = new CreateWorkspaceRequest("Home", WorkspaceType.Household, "Shared tasks");

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithoutDescription_IsValid()
    {
        var request = new CreateWorkspaceRequest("Home", WorkspaceType.Personal, null);

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_WithBlankName_ReportsTheNameField(string name)
    {
        var request = new CreateWorkspaceRequest(name, WorkspaceType.Personal, null);

        var result = _validator.Validate(request);

        Assert.Equal(
            [nameof(CreateWorkspaceRequest.Name)],
            result.Errors.Select(error => error.PropertyName).Distinct());
    }

    [Fact]
    public void Validate_WithNameOfTheMaximumLength_IsValid()
    {
        var request = new CreateWorkspaceRequest(
            new string('a', CreateWorkspaceRequestValidator.NameMaxLength),
            WorkspaceType.Personal,
            null);

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithNameLongerThanTheLimit_ReportsTheNameField()
    {
        var request = new CreateWorkspaceRequest(
            new string('a', CreateWorkspaceRequestValidator.NameMaxLength + 1),
            WorkspaceType.Personal,
            null);

        var result = _validator.Validate(request);

        Assert.Equal(
            [nameof(CreateWorkspaceRequest.Name)],
            result.Errors.Select(error => error.PropertyName).Distinct());
    }

    [Fact]
    public void Validate_WithUnknownType_ReportsTheTypeField()
    {
        var request = new CreateWorkspaceRequest("Home", (WorkspaceType)99, null);

        var result = _validator.Validate(request);

        Assert.Equal(
            [nameof(CreateWorkspaceRequest.Type)],
            result.Errors.Select(error => error.PropertyName).Distinct());
    }

    [Fact]
    public void Validate_WithDescriptionOfTheMaximumLength_IsValid()
    {
        var request = new CreateWorkspaceRequest(
            "Home",
            WorkspaceType.Personal,
            new string('a', CreateWorkspaceRequestValidator.DescriptionMaxLength));

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Validate_WithDescriptionLongerThanTheLimit_ReportsTheDescriptionField()
    {
        var request = new CreateWorkspaceRequest(
            "Home",
            WorkspaceType.Personal,
            new string('a', CreateWorkspaceRequestValidator.DescriptionMaxLength + 1));

        var result = _validator.Validate(request);

        Assert.Equal(
            [nameof(CreateWorkspaceRequest.Description)],
            result.Errors.Select(error => error.PropertyName).Distinct());
    }
}
