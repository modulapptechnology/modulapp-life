using System.Net;
using Life.Flows.Tests.Support;

namespace Life.Flows.Tests.Flows;

public class FlowStepTests
{
    [Fact]
    public async Task ExpectAsync_WithTheExpectedStatus_ReturnsTheResponse()
    {
        var response = new HttpResponseMessage(HttpStatusCode.Created);

        var result = await Task.FromResult(response).ExpectAsync("create", HttpStatusCode.Created);

        Assert.Same(response, result);
    }

    [Fact]
    public async Task ExpectAsync_WithAnotherStatus_ReportsTheStepTheStatusAndTheResponse()
    {
        var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
        {
            Content = new StringContent("{\"title\":\"Invalid name\"}"),
        };

        var failure = await Assert.ThrowsAsync<FlowStepFailedException>(
            () => Task.FromResult(response).ExpectAsync("create workspace", HttpStatusCode.Created));

        Assert.Equal(
            "Flow step 'create workspace' failed: expected 201 Created "
            + "but received 400 BadRequest. Response: {\"title\":\"Invalid name\"}",
            failure.Message);
    }

    [Fact]
    public async Task ExpectAsync_WithALongResponse_TruncatesTheReportedBody()
    {
        var response = new HttpResponseMessage(HttpStatusCode.InternalServerError)
        {
            Content = new StringContent(new string('x', 2000)),
        };

        var failure = await Assert.ThrowsAsync<FlowStepFailedException>(
            () => Task.FromResult(response).ExpectAsync("login", HttpStatusCode.OK));

        Assert.EndsWith(new string('x', 500), failure.Message);
        Assert.DoesNotContain(new string('x', 501), failure.Message);
    }
}
