using Xunit;
using System.IdentityModel.Tokens.Jwt;
using Ez.Reasons.Core.Models;
using Ez.Reasons.Core.Repositories;
using Ez.Reasons.Core.Services;
using Moq;

namespace Ez.Reasons.Core.Tests;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _mockRepo;
    private readonly AuthService _service;
    private const string JwtSecret = "this-is-a-test-secret-that-is-at-least-32-characters-long!!";

    public AuthServiceTests()
    {
        _mockRepo = new Mock<IUserRepository>();
        _service = new AuthService(_mockRepo.Object, JwtSecret);
    }

    [Fact]
    public async Task Login_ReturnsToken_ForValidCredentials()
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("correct-password");
        var user = new User
        {
            Username = "admin",
            PasswordHash = passwordHash,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _mockRepo.Setup(r => r.GetByUsernameAsync("admin")).ReturnsAsync(user);

        var result = await _service.LoginAsync(new LoginRequest("admin", "correct-password"));

        Assert.NotNull(result);
        Assert.False(string.IsNullOrEmpty(result.Token));
    }

    [Fact]
    public async Task Login_ReturnsNull_ForInvalidPassword()
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("correct-password");
        var user = new User
        {
            Username = "admin",
            PasswordHash = passwordHash,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _mockRepo.Setup(r => r.GetByUsernameAsync("admin")).ReturnsAsync(user);

        var result = await _service.LoginAsync(new LoginRequest("admin", "wrong-password"));

        Assert.Null(result);
    }

    [Fact]
    public async Task Login_ReturnsNull_ForNonExistentUser()
    {
        _mockRepo.Setup(r => r.GetByUsernameAsync("nobody")).ReturnsAsync((User?)null);

        var result = await _service.LoginAsync(new LoginRequest("nobody", "password"));

        Assert.Null(result);
    }

    [Fact]
    public async Task Login_TokenContainsCorrectClaims()
    {
        var passwordHash = BCrypt.Net.BCrypt.HashPassword("password");
        var user = new User
        {
            Username = "admin",
            PasswordHash = passwordHash,
            CreatedAt = DateTimeOffset.UtcNow
        };
        _mockRepo.Setup(r => r.GetByUsernameAsync("admin")).ReturnsAsync(user);

        var result = await _service.LoginAsync(new LoginRequest("admin", "password"));

        Assert.NotNull(result);

        var handler = new JwtSecurityTokenHandler();
        var token = handler.ReadJwtToken(result.Token);

        var sub = token.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;
        var role = token.Claims.FirstOrDefault(c => c.Type == "role")?.Value;

        Assert.Equal("admin", sub);
        Assert.Equal("moderator", role);
        Assert.True(token.ValidTo > DateTime.UtcNow.AddHours(23));
        Assert.True(token.ValidTo <= DateTime.UtcNow.AddHours(25));
    }
}
