namespace Ez.Reasons.Core.Models;

public record NextLetterRequest(List<string> SeenIds);
public record NextLetterResponse(string Id, string Title, string Body, DateTimeOffset SubmittedAt);
public record SubmitLetterRequest(string Title, string Body, string? Email);
public record FeedbackRequest(string Type); // "like" or "dislike"
public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token);
public record PendingLetterResponse(string Id, string Title, string Body, string? Email, DateTimeOffset SubmittedAt);
public record ErrorResponse(string Error);
