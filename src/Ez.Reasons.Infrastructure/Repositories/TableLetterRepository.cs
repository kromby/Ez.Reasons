using Azure;
using Azure.Data.Tables;
using Ez.Reasons.Core.Models;
using Ez.Reasons.Core.Repositories;
using Ez.Reasons.Infrastructure.Entities;
using Ez.Reasons.Infrastructure.Mappers;

namespace Ez.Reasons.Infrastructure.Repositories;

public class TableLetterRepository : ILetterRepository
{
    private readonly TableClient _tableClient;

    public TableLetterRepository(TableServiceClient serviceClient)
    {
        _tableClient = serviceClient.GetTableClient("Letters");
        _tableClient.CreateIfNotExists();
    }

    public async Task<List<Letter>> GetApprovedAsync()
    {
        var letters = new List<Letter>();
        await foreach (var entity in _tableClient.QueryAsync<LetterEntity>(e => e.PartitionKey == "approved"))
        {
            letters.Add(EntityMappers.ToDomain(entity));
        }
        return letters;
    }

    public async Task CreateAsync(Letter letter)
    {
        var entity = EntityMappers.ToEntity(letter);
        await _tableClient.AddEntityAsync(entity);
    }

    public async Task<List<Letter>> GetPendingAsync()
    {
        var letters = new List<Letter>();
        await foreach (var entity in _tableClient.QueryAsync<LetterEntity>(e => e.PartitionKey == "pending"))
        {
            letters.Add(EntityMappers.ToDomain(entity));
        }
        return letters;
    }

    public async Task MoveToStatusAsync(string id, string currentStatus, string newStatus, string? reviewedBy)
    {
        var response = await _tableClient.GetEntityAsync<LetterEntity>(currentStatus, id);
        var entity = response.Value;

        // Create new entity in the target partition (insert first to avoid data loss)
        var newEntity = new LetterEntity
        {
            PartitionKey = newStatus,
            RowKey = entity.RowKey,
            Title = entity.Title,
            Body = entity.Body,
            Email = entity.Email,
            SubmittedAt = entity.SubmittedAt,
            ReviewedAt = DateTimeOffset.UtcNow,
            ReviewedBy = reviewedBy,
            ViewCount = entity.ViewCount,
            LikeCount = entity.LikeCount,
            DislikeCount = entity.DislikeCount
        };

        await _tableClient.AddEntityAsync(newEntity);

        // Then delete from old partition
        await _tableClient.DeleteEntityAsync(currentStatus, id, entity.ETag);
    }

    public async Task<Letter?> GetByIdAsync(string id, string status)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<LetterEntity>(status, id);
            return EntityMappers.ToDomain(response.Value);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }

    public async Task IncrementViewCountAsync(string id)
    {
        await IncrementCounterAsync(id, entity => entity.ViewCount++);
    }

    public async Task IncrementLikeCountAsync(string id)
    {
        await IncrementCounterAsync(id, entity => entity.LikeCount++);
    }

    public async Task IncrementDislikeCountAsync(string id)
    {
        await IncrementCounterAsync(id, entity => entity.DislikeCount++);
    }

    private async Task IncrementCounterAsync(string id, Action<LetterEntity> incrementAction)
    {
        const int maxRetries = 2;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            try
            {
                var response = await _tableClient.GetEntityAsync<LetterEntity>("approved", id);
                var entity = response.Value;
                incrementAction(entity);
                await _tableClient.UpdateEntityAsync(entity, entity.ETag, TableUpdateMode.Replace);
                return;
            }
            catch (RequestFailedException ex) when (ex.Status == 412 && attempt < maxRetries - 1)
            {
                // ETag conflict, retry once
            }
        }
    }
}
