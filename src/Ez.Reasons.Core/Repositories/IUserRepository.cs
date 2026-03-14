namespace Ez.Reasons.Core.Repositories;

using Ez.Reasons.Core.Models;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username);
}
