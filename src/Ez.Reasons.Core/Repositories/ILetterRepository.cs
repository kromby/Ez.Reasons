namespace Ez.Reasons.Core.Repositories;

using Ez.Reasons.Core.Models;

public interface ILetterRepository
{
    Task<List<Letter>> GetApprovedAsync();
    Task CreateAsync(Letter letter);
    Task<List<Letter>> GetPendingAsync();
    Task MoveToStatusAsync(string id, string currentStatus, string newStatus, string? reviewedBy);
    Task<Letter?> GetByIdAsync(string id, string status);
    Task IncrementViewCountAsync(string id);
    Task IncrementLikeCountAsync(string id);
    Task IncrementDislikeCountAsync(string id);
}
