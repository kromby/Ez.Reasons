namespace Ez.Reasons.Core.Services;

using Ez.Reasons.Core.Models;

public interface ILetterService
{
    Task<NextLetterResponse?> GetNextLetterAsync(List<string> seenIds);
    Task SubmitLetterAsync(SubmitLetterRequest request);
    Task SubmitFeedbackAsync(string letterId, string feedbackType);
    Task<List<PendingLetterResponse>> GetPendingLettersAsync();
    Task ApproveLetterAsync(string id, string reviewedBy);
    Task RejectLetterAsync(string id, string reviewedBy);
}
