using Azure;
using Azure.Data.Tables;
using Ez.Reasons.Core.Models;
using Ez.Reasons.Core.Repositories;
using Ez.Reasons.Infrastructure.Entities;
using Ez.Reasons.Infrastructure.Mappers;

namespace Ez.Reasons.Infrastructure.Repositories;

public class TableUserRepository : IUserRepository
{
    private readonly TableClient _tableClient;

    public TableUserRepository(TableServiceClient serviceClient)
    {
        _tableClient = serviceClient.GetTableClient("Users");
        _tableClient.CreateIfNotExists();
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        try
        {
            var response = await _tableClient.GetEntityAsync<UserEntity>("moderator", username.ToLower());
            return EntityMappers.ToDomain(response.Value);
        }
        catch (RequestFailedException ex) when (ex.Status == 404)
        {
            return null;
        }
    }
}
