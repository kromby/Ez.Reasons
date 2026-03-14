using System.Net;
using Ez.Reasons.Api.Extensions;
using Ez.Reasons.Core.Models;
using Ez.Reasons.Core.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Ez.Reasons.Api.Functions;

public class ModerationFunctions
{
    private readonly ILetterService _letterService;

    public ModerationFunctions(ILetterService letterService)
    {
        _letterService = letterService;
    }

    [Function("GetPendingLetters")]
    public async Task<HttpResponseData> GetPendingLetters(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "moderation/pending")] HttpRequestData req,
        FunctionContext context)
    {
        try
        {
            var pending = await _letterService.GetPendingLettersAsync();
            return await req.CreateJsonResponse(HttpStatusCode.OK, pending);
        }
        catch (Exception ex)
        {
            return await req.CreateJsonResponse(HttpStatusCode.InternalServerError, new ErrorResponse(ex.Message));
        }
    }

    [Function("ApproveLetter")]
    public async Task<HttpResponseData> ApproveLetter(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "moderation/{id}/approve")] HttpRequestData req,
        string id,
        FunctionContext context)
    {
        try
        {
            var username = context.Items.TryGetValue("username", out var user)
                ? user as string ?? "unknown"
                : "unknown";

            await _letterService.ApproveLetterAsync(id, username);

            return req.CreateResponse(HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            return await req.CreateJsonResponse(HttpStatusCode.InternalServerError, new ErrorResponse(ex.Message));
        }
    }

    [Function("RejectLetter")]
    public async Task<HttpResponseData> RejectLetter(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "moderation/{id}/reject")] HttpRequestData req,
        string id,
        FunctionContext context)
    {
        try
        {
            var username = context.Items.TryGetValue("username", out var user)
                ? user as string ?? "unknown"
                : "unknown";

            await _letterService.RejectLetterAsync(id, username);

            return req.CreateResponse(HttpStatusCode.OK);
        }
        catch (Exception ex)
        {
            return await req.CreateJsonResponse(HttpStatusCode.InternalServerError, new ErrorResponse(ex.Message));
        }
    }
}
