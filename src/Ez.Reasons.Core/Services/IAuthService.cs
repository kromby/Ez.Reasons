namespace Ez.Reasons.Core.Services;

using Ez.Reasons.Core.Models;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
}
