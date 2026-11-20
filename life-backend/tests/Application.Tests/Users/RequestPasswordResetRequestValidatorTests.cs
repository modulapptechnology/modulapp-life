using Life.Application.Users;

namespace Life.Application.Tests.Users;

public class RequestPasswordResetRequestValidatorTests
{
    private readonly RequestPasswordResetRequestValidator _validator = new();

    [Fact]
    public void Validate_WithAnEmail_IsValid()
    {
        var request = new RequestPasswordResetRequest("person@life.test");

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    public void Validate_WithInvalidEmail_ReportsTheEmailField(string email)
    {
        var request = new RequestPasswordResetRequest(email);

        var result = _validator.Validate(request);

        Assert.Equal(
            [nameof(RequestPasswordResetRequest.Email)],
            result.Errors.Select(error => error.PropertyName).Distinct());
    }
}
