using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Text;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.Functions.Worker.Middleware;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace Ez.Reasons.Api.Middleware;

public class JwtMiddleware : IFunctionsWorkerMiddleware
{
    public async Task Invoke(FunctionContext context, FunctionExecutionDelegate next)
    {
        var requestData = await context.GetHttpRequestDataAsync();

        if (requestData != null)
        {
            var functionName = context.FunctionDefinition.Name;

            // Only protect moderation routes
            if (functionName is "GetPendingLetters" or "ApproveLetter" or "RejectLetter")
            {
                var token = requestData.Headers.TryGetValues("X-Auth-Token", out var values)
                    ? values.FirstOrDefault()
                    : null;

                if (string.IsNullOrEmpty(token))
                {
                    var response = requestData.CreateResponse(HttpStatusCode.Unauthorized);
                    await response.WriteAsJsonAsync(new { error = "Unauthorized" });
                    context.GetInvocationResult().Value = response;
                    return;
                }

                var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")!;

                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var key = Encoding.UTF8.GetBytes(jwtSecret);
                    var validationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(key),
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.FromMinutes(5)
                    };

                    var principal = tokenHandler.ValidateToken(token, validationParameters, out _);

                    var username = principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                        ?? principal.FindFirst("sub")?.Value;
                    var role = principal.FindFirst("role")?.Value;

                    if (username != null)
                        context.Items["username"] = username;
                    if (role != null)
                        context.Items["role"] = role;
                }
                catch (Exception ex)
                {
                    var logger = context.GetLogger<JwtMiddleware>();
                    logger.LogError(ex, "JWT validation failed for function {FunctionName}", functionName);

                    var response = requestData.CreateResponse(HttpStatusCode.Unauthorized);
                    await response.WriteAsJsonAsync(new { error = "Invalid or expired token" });
                    context.GetInvocationResult().Value = response;
                    return;
                }
            }
        }

        await next(context);
    }
}
