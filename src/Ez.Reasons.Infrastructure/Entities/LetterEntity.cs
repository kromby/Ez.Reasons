using Azure;
using Azure.Data.Tables;

namespace Ez.Reasons.Infrastructure.Entities;

public class LetterEntity : ITableEntity
{
    public string PartitionKey { get; set; } = string.Empty; // status: pending/approved/rejected
    public string RowKey { get; set; } = string.Empty; // GUID
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string? Email { get; set; }
    public DateTimeOffset SubmittedAt { get; set; }
    public DateTimeOffset? ReviewedAt { get; set; }
    public string? ReviewedBy { get; set; }
    public int ViewCount { get; set; }
    public int LikeCount { get; set; }
    public int DislikeCount { get; set; }
}
