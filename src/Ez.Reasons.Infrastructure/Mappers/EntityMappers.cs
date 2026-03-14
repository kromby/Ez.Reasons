using Ez.Reasons.Core.Models;
using Ez.Reasons.Infrastructure.Entities;

namespace Ez.Reasons.Infrastructure.Mappers;

public static class EntityMappers
{
    public static Letter ToDomain(LetterEntity entity)
    {
        return new Letter
        {
            Id = entity.RowKey,
            Title = entity.Title,
            Body = entity.Body,
            Email = entity.Email,
            Status = entity.PartitionKey,
            SubmittedAt = entity.SubmittedAt,
            ReviewedAt = entity.ReviewedAt,
            ReviewedBy = entity.ReviewedBy,
            ViewCount = entity.ViewCount,
            LikeCount = entity.LikeCount,
            DislikeCount = entity.DislikeCount
        };
    }

    public static LetterEntity ToEntity(Letter letter)
    {
        return new LetterEntity
        {
            PartitionKey = letter.Status,
            RowKey = letter.Id,
            Title = letter.Title,
            Body = letter.Body,
            Email = letter.Email,
            SubmittedAt = letter.SubmittedAt,
            ReviewedAt = letter.ReviewedAt,
            ReviewedBy = letter.ReviewedBy,
            ViewCount = letter.ViewCount,
            LikeCount = letter.LikeCount,
            DislikeCount = letter.DislikeCount
        };
    }

    public static User ToDomain(UserEntity entity)
    {
        return new User
        {
            Username = entity.RowKey,
            PasswordHash = entity.PasswordHash,
            CreatedAt = entity.CreatedAt
        };
    }

    public static UserEntity ToEntity(User user)
    {
        return new UserEntity
        {
            PartitionKey = "moderator",
            RowKey = user.Username.ToLower(),
            PasswordHash = user.PasswordHash,
            CreatedAt = user.CreatedAt
        };
    }
}
