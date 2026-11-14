using System.Net;
using System.Net.Http.Json;
using Life.Flows.Tests.Support;
using Life.Support;

namespace Life.Flows.Tests.Flows;

[Collection(FlowsCollection.Name)]
public class AuthenticationFlowTests(FlowsFactory api)
{
    [RequiresTestDatabaseFact]
    public async Task SignUp_WithRegisterVerifyAndLogin_ReturnsASessionThatReachesProtectedRoutes()
    {
        var user = await FlowUser.SignUpAsync(api, "signup");

        var response = await user.Client.GetAsync(WorkspaceFlow.Route);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [RequiresTestDatabaseFact]
    public async Task Login_BeforeVerifyingTheEmail_Returns401()
    {
        var email = FlowUser.NewEmail("unverified");
        await FlowUser.RegisterAsync(api, email);

        var response = await api.CreateClient()
            .PostAsJsonAsync("/api/v1/Auth/login", new { email, password = FlowUser.Password });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [RequiresTestDatabaseFact]
    public async Task Login_WithAWrongPassword_Returns401()
    {
        var user = await FlowUser.SignUpAsync(api, "wrongpass");

        var response = await api.CreateClient()
            .PostAsJsonAsync("/api/v1/Auth/login", new { email = user.Email, password = "Wrongpass99" });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [RequiresTestDatabaseFact]
    public async Task Register_WithAnEmailAlreadyRegistered_Returns409()
    {
        var user = await FlowUser.SignUpAsync(api, "duplicate");

        var response = await api.CreateClient()
            .PostAsJsonAsync("/api/v1/Auth/register", new { email = user.Email, password = FlowUser.Password });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [RequiresTestDatabaseFact]
    public async Task Verify_WithAnInvalidCode_Returns400()
    {
        var email = FlowUser.NewEmail("badcode");
        await FlowUser.RegisterAsync(api, email);

        var response = await api.CreateClient()
            .PostAsJsonAsync("/api/v1/Auth/verify-email", new { email, code = "000000" });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
