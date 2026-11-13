using System.Net;
using System.Net.Http.Json;
using API.Tests.Support;

namespace API.Tests.House;

[Collection(ApiCollection.Name)]
public class HouseEndpointTests(ApiFactory api)
{
    private sealed record StatusResponse(string Status);

    [Fact]
    public async Task Get_VersionedRoute_Returns200WithOkStatus()
    {
        var client = api.CreateApiClient();

        var response = await client.GetAsync("/api/v1/house");

        var body = await response.Content.ReadFromJsonAsync<StatusResponse>();
        Assert.Equal((HttpStatusCode.OK, "ok"), (response.StatusCode, body?.Status));
    }

    [Fact]
    public async Task Get_UnsupportedVersion_Returns404()
    {
        var client = api.CreateApiClient(Guid.NewGuid());

        var response = await client.GetAsync("/api/v9/house");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_MethodNotAllowed_Returns405()
    {
        var client = api.CreateApiClient(Guid.NewGuid());

        var response = await client.PostAsync("/api/v1/house", content: null);

        Assert.Equal(HttpStatusCode.MethodNotAllowed, response.StatusCode);
    }
}
