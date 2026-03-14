using System.Net;
using System.Text.Json;
using Microsoft.Azure.Functions.Worker.Http;

namespace Ez.Reasons.Api.Extensions;

public static class HttpResponseExtensions
{
    private static readonly JsonSerializerOptions CamelCase = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static async Task WriteJsonAsync<T>(this HttpResponseData response, T value)
    {
        response.Headers.Add("Content-Type", "application/json; charset=utf-8");
        var json = JsonSerializer.Serialize(value, CamelCase);
        await response.WriteStringAsync(json);
    }

    public static async Task<HttpResponseData> CreateJsonResponse<T>(
        this HttpRequestData req, HttpStatusCode statusCode, T value)
    {
        var response = req.CreateResponse(statusCode);
        await response.WriteJsonAsync(value);
        return response;
    }
}
