namespace Ez.Reasons.Core.Services;

using System.Text.RegularExpressions;
using Ez.Reasons.Core.Models;
using Ez.Reasons.Core.Repositories;

public class LetterService : ILetterService
{
    private readonly ILetterRepository _letterRepository;
    private static readonly Random _random = new();

    public LetterService(ILetterRepository letterRepository)
    {
        _letterRepository = letterRepository;
    }

    public async Task<NextLetterResponse?> GetNextLetterAsync(List<string> seenIds)
    {
        var approved = await _letterRepository.GetApprovedAsync();
        if (approved.Count == 0)
            return null;

        var candidates = approved.Where(l => !seenIds.Contains(l.Id)).ToList();

        // Fall back to all approved when all have been seen
        if (candidates.Count == 0)
            candidates = approved;

        // Weighted random selection by quality score (likes - dislikes), minimum weight of 1
        var weights = candidates.Select(l =>
        {
            var score = l.LikeCount - l.DislikeCount;
            return Math.Max(1, score);
        }).ToList();

        var totalWeight = weights.Sum();
        var randomValue = _random.Next(totalWeight);
        var cumulativeWeight = 0;
        Letter selected = candidates[0];

        for (int i = 0; i < candidates.Count; i++)
        {
            cumulativeWeight += weights[i];
            if (randomValue < cumulativeWeight)
            {
                selected = candidates[i];
                break;
            }
        }

        await _letterRepository.IncrementViewCountAsync(selected.Id);

        return new NextLetterResponse(selected.Id, selected.Title, selected.Body, selected.SubmittedAt);
    }

    public async Task SubmitLetterAsync(SubmitLetterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            throw new ArgumentException("Title is required.");

        if (request.Title.Length > 200)
            throw new ArgumentException("Title must not exceed 200 characters.");

        if (string.IsNullOrWhiteSpace(request.Body))
            throw new ArgumentException("Body is required.");

        if (request.Body.Length > 5000)
            throw new ArgumentException("Body must not exceed 5000 characters.");

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            if (!IsValidEmail(request.Email))
                throw new ArgumentException("Email format is invalid.");
        }

        var letter = new Letter
        {
            Id = Guid.NewGuid().ToString(),
            Title = request.Title,
            Body = request.Body,
            Email = request.Email,
            Status = "pending",
            SubmittedAt = DateTimeOffset.UtcNow
        };

        await _letterRepository.CreateAsync(letter);
    }

    public async Task SubmitFeedbackAsync(string letterId, string feedbackType)
    {
        if (feedbackType != "like" && feedbackType != "dislike")
            throw new ArgumentException("Feedback type must be 'like' or 'dislike'.");

        if (feedbackType == "like")
            await _letterRepository.IncrementLikeCountAsync(letterId);
        else
            await _letterRepository.IncrementDislikeCountAsync(letterId);
    }

    public async Task<List<PendingLetterResponse>> GetPendingLettersAsync()
    {
        var pending = await _letterRepository.GetPendingAsync();
        return pending.Select(l => new PendingLetterResponse(l.Id, l.Title, l.Body, l.Email, l.SubmittedAt)).ToList();
    }

    public async Task ApproveLetterAsync(string id, string reviewedBy)
    {
        await _letterRepository.MoveToStatusAsync(id, "pending", "approved", reviewedBy);
    }

    public async Task RejectLetterAsync(string id, string reviewedBy)
    {
        await _letterRepository.MoveToStatusAsync(id, "pending", "rejected", reviewedBy);
    }

    private static bool IsValidEmail(string email)
    {
        var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }
}
