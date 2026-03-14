using System.Net;
using System.Text.Json;
using Ez.Reasons.Api.Extensions;
using Ez.Reasons.Core.Models;
using Ez.Reasons.Core.Services;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;

namespace Ez.Reasons.Api.Functions;

public class LetterFunctions
{
    private readonly ILetterService _letterService;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public LetterFunctions(ILetterService letterService)
    {
        _letterService = letterService;
    }

    [Function("GetNextLetter")]
    public async Task<HttpResponseData> GetNextLetter(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "letters/next")] HttpRequestData req)
    {
        try
        {
            var body = await req.ReadAsStringAsync();
            var request = JsonSerializer.Deserialize<NextLetterRequest>(body ?? "{}", _jsonOptions);
            var seenIds = request?.SeenIds ?? new List<string>();

            var result = await _letterService.GetNextLetterAsync(seenIds);

            if (result == null)
                return await req.CreateJsonResponse(HttpStatusCode.NotFound, new ErrorResponse("No letters available"));

            return await req.CreateJsonResponse(HttpStatusCode.OK, result);
        }
        catch (Exception ex)
        {
            return await req.CreateJsonResponse(HttpStatusCode.InternalServerError, new ErrorResponse(ex.Message));
        }
    }

    [Function("SubmitLetter")]
    public async Task<HttpResponseData> SubmitLetter(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "letters")] HttpRequestData req)
    {
        try
        {
            var body = await req.ReadAsStringAsync();
            var request = JsonSerializer.Deserialize<SubmitLetterRequest>(body ?? "{}", _jsonOptions);

            if (request == null)
                return await req.CreateJsonResponse(HttpStatusCode.BadRequest, new ErrorResponse("Invalid request body"));

            await _letterService.SubmitLetterAsync(request);

            return req.CreateResponse(HttpStatusCode.Created);
        }
        catch (ArgumentException ex)
        {
            return await req.CreateJsonResponse(HttpStatusCode.BadRequest, new ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return await req.CreateJsonResponse(HttpStatusCode.InternalServerError, new ErrorResponse(ex.Message));
        }
    }

    [Function("SubmitFeedback")]
    public async Task<HttpResponseData> SubmitFeedback(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "letters/{id}/feedback")] HttpRequestData req,
        string id)
    {
        try
        {
            var body = await req.ReadAsStringAsync();
            var request = JsonSerializer.Deserialize<FeedbackRequest>(body ?? "{}", _jsonOptions);

            if (request == null)
                return await req.CreateJsonResponse(HttpStatusCode.BadRequest, new ErrorResponse("Invalid request body"));

            await _letterService.SubmitFeedbackAsync(id, request.Type);

            return req.CreateResponse(HttpStatusCode.OK);
        }
        catch (ArgumentException ex)
        {
            return await req.CreateJsonResponse(HttpStatusCode.BadRequest, new ErrorResponse(ex.Message));
        }
        catch (Exception ex)
        {
            return await req.CreateJsonResponse(HttpStatusCode.InternalServerError, new ErrorResponse(ex.Message));
        }
    }
}
