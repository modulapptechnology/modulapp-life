using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using API.Tests.Support;

namespace API.Tests.Validation;

[Collection(ApiCollection.Name)]
public class ValidationTests(ApiFactory api)
{
    private const string _route = "/api/v1/TestValidation";

    [Fact]
    public async Task Post_ValidRequest_Returns200WithTheResponseDto()
    {
        var client = api.CreateApiClient();

        var response = await client.PostAsJsonAsync(_route, new { name = "Sample", quantity = 5 });

        var body = await response.Content.ReadFromJsonAsync<SampleResponse>();
        Assert.Equal((HttpStatusCode.OK, "Sample", 5), (response.StatusCode, body?.Name, body?.Quantity));
    }

    [Fact]
    public async Task Post_EmptyName_Returns400ProblemDetailsWithTheInvalidField()
    {
        var client = api.CreateApiClient();

        var response = await client.PostAsJsonAsync(_route, new { name = "", quantity = 5 });

        var errors = await ReadErrorsAsync(response);
        Assert.Equal((HttpStatusCode.BadRequest, "application/problem+json", true, false),
            (
                response.StatusCode,
                response.Content.Headers.ContentType?.MediaType,
                errors.ContainsKey("Name"),
                errors.ContainsKey("Quantity")));
    }

    [Fact]
    public async Task Post_SeveralInvalidFields_ReturnsAllOfThemInTheSameResponse()
    {
        var client = api.CreateApiClient();

        var response = await client.PostAsJsonAsync(_route, new { name = "", quantity = 0 });

        var errors = await ReadErrorsAsync(response);
        Assert.Equal(
            (HttpStatusCode.BadRequest, true, true),
            (response.StatusCode, errors.ContainsKey("Name"), errors.ContainsKey("Quantity")));
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(1, true)]
    [InlineData(100, true)]
    [InlineData(101, false)]
    public async Task Post_QuantityAtTheLimits_AcceptsOnlyTheAllowedRange(int quantity, bool accepted)
    {
        var client = api.CreateApiClient();

        var response = await client.PostAsJsonAsync(_route, new { name = "Sample", quantity });

        Assert.Equal(accepted, response.StatusCode == HttpStatusCode.OK);
    }

    [Fact]
    public async Task Post_WrongTypeInBody_Returns400WithoutInternalDetails()
    {
        var client = api.CreateApiClient();

        var response = await client.PostAsJsonAsync(_route, new { name = "Sample", quantity = "abc" });

        var body = await response.Content.ReadAsStringAsync();
        Assert.Equal(
            (HttpStatusCode.BadRequest, false, false),
            (response.StatusCode, body.Contains("System."), body.Contains("LineNumber")));
    }

    [Fact]
    public async Task Post_EmptyJsonObject_Returns400WithTheMissingFields()
    {
        var client = api.CreateApiClient();

        var response = await client.PostAsync(
            _route,
            new StringContent("{}", System.Text.Encoding.UTF8, "application/json"));

        var errors = await ReadErrorsAsync(response);
        Assert.Equal((HttpStatusCode.BadRequest, true), (response.StatusCode, errors.ContainsKey("Name")));
    }

    private static async Task<Dictionary<string, string[]>> ReadErrorsAsync(HttpResponseMessage response)
    {
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var errors = document.RootElement.GetProperty("errors");

        return errors.EnumerateObject().ToDictionary(
            property => property.Name,
            property => property.Value.EnumerateArray()
                .Select(item => item.GetString() ?? string.Empty)
                .ToArray());
    }
}
