using System.Net;

namespace Life.Flows.Tests.Support;

public static class FlowStep
{
    private const int _maxBodyLength = 500;

    public static async Task<HttpResponseMessage> ExpectAsync(
        this Task<HttpResponseMessage> request,
        string step,
        HttpStatusCode expected)
    {
        var response = await request;

        if (response.StatusCode == expected)
        {
            return response;
        }

        var body = await response.Content.ReadAsStringAsync();

        throw new FlowStepFailedException(step, expected, response.StatusCode, Truncate(body));
    }

    private static string Truncate(string body)
    {
        return body.Length <= _maxBodyLength ? body : body[.._maxBodyLength];
    }
}

public sealed class FlowStepFailedException(
    string step,
    HttpStatusCode expected,
    HttpStatusCode received,
    string body)
    : Exception(
        $"Flow step '{step}' failed: expected {(int)expected} {expected} "
        + $"but received {(int)received} {received}. Response: {body}");
