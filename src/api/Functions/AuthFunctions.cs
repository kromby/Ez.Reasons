using System.Net;
using System.Text.Json;
using Ez.Reasons.Api.Extensions;
using Ez.Reasons.Core.Models;
using Ez.Reasons.Core.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Ez.Reasons.Api.Functions;

public class AuthFunctions
{
    private readonly IAuthService _authService;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public AuthFunctions(IAuthService authService)
    {
        _authService = authService;
    }

    [Function("Login")]
    public async Task<HttpResponseData> Login(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/login")] HttpRequestData req)
    {
        try
        {
            var body = await req.ReadAsStringAsync();
            var request = JsonSerializer.Deserialize<LoginRequest>(body ?? "{}", _jsonOptions);

            if (request == null || string.IsNullOrEmpty(request.Username) || string.IsNullOrEmpty(request.Password))
                return await req.CreateJsonResponse(HttpStatusCode.BadRequest, new ErrorResponse("Username and password are required"));

            var result = await _authService.LoginAsync(request);

            if (result == null)
                return await req.CreateJsonResponse(HttpStatusCode.Unauthorized, new ErrorResponse("Invalid username or password"));

            return await req.CreateJsonResponse(HttpStatusCode.OK, result);
        }
        catch (Exception ex)
        {
            return await req.CreateJsonResponse(HttpStatusCode.InternalServerError, new ErrorResponse(ex.Message));
        }
    }
}
