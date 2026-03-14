using Azure;
using Azure.Data.Tables;

namespace Ez.Reasons.Infrastructure.Entities;

public class UserEntity : ITableEntity
{
    public string PartitionKey { get; set; } = "moderator";
    public string RowKey { get; set; } = string.Empty; // username, lowercase
    public DateTimeOffset? Timestamp { get; set; }
    public ETag ETag { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
