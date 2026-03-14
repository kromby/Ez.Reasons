using Xunit;
using Ez.Reasons.Core.Models;
using Ez.Reasons.Core.Repositories;
using Ez.Reasons.Core.Services;
using Moq;

namespace Ez.Reasons.Core.Tests;

public class LetterServiceTests
{
    private readonly Mock<ILetterRepository> _mockRepo;
    private readonly LetterService _service;

    public LetterServiceTests()
    {
        _mockRepo = new Mock<ILetterRepository>();
        _service = new LetterService(_mockRepo.Object);
    }

    [Fact]
    public async Task GetNextLetter_ReturnsLetter_WhenApprovedLettersExist()
    {
        var letters = new List<Letter>
        {
            new() { Id = "1", Title = "Hello", Body = "World", Status = "approved", SubmittedAt = DateTimeOffset.UtcNow }
        };
        _mockRepo.Setup(r => r.GetApprovedAsync()).ReturnsAsync(letters);

        var result = await _service.GetNextLetterAsync(new List<string>());

        Assert.NotNull(result);
        Assert.Equal("1", result.Id);
        Assert.Equal("Hello", result.Title);
        Assert.Equal("World", result.Body);
    }

    [Fact]
    public async Task GetNextLetter_ExcludesSeenIds()
    {
        var letters = new List<Letter>
        {
            new() { Id = "1", Title = "Seen", Body = "Body1", Status = "approved", SubmittedAt = DateTimeOffset.UtcNow },
            new() { Id = "2", Title = "Unseen", Body = "Body2", Status = "approved", SubmittedAt = DateTimeOffset.UtcNow }
        };
        _mockRepo.Setup(r => r.GetApprovedAsync()).ReturnsAsync(letters);

        var result = await _service.GetNextLetterAsync(new List<string> { "1" });

        Assert.NotNull(result);
        Assert.Equal("2", result.Id);
    }

    [Fact]
    public async Task GetNextLetter_FallsBackToAllApproved_WhenAllSeen()
    {
        var letters = new List<Letter>
        {
            new() { Id = "1", Title = "Hello", Body = "World", Status = "approved", SubmittedAt = DateTimeOffset.UtcNow }
        };
        _mockRepo.Setup(r => r.GetApprovedAsync()).ReturnsAsync(letters);

        var result = await _service.GetNextLetterAsync(new List<string> { "1" });

        Assert.NotNull(result);
        Assert.Equal("1", result.Id);
    }

    [Fact]
    public async Task GetNextLetter_ReturnsNull_WhenNoApprovedLettersExist()
    {
        _mockRepo.Setup(r => r.GetApprovedAsync()).ReturnsAsync(new List<Letter>());

        var result = await _service.GetNextLetterAsync(new List<string>());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetNextLetter_IncrementsViewCount()
    {
        var letters = new List<Letter>
        {
            new() { Id = "1", Title = "Hello", Body = "World", Status = "approved", SubmittedAt = DateTimeOffset.UtcNow }
        };
        _mockRepo.Setup(r => r.GetApprovedAsync()).ReturnsAsync(letters);

        await _service.GetNextLetterAsync(new List<string>());

        _mockRepo.Verify(r => r.IncrementViewCountAsync("1"), Times.Once);
    }

    [Fact]
    public async Task GetNextLetter_UsesWeightedSelection_HigherScoreMoreLikely()
    {
        // Create one letter with very high score and one with zero score
        var letters = new List<Letter>
        {
            new() { Id = "high", Title = "High", Body = "Body", Status = "approved", LikeCount = 1000, DislikeCount = 0, SubmittedAt = DateTimeOffset.UtcNow },
            new() { Id = "low", Title = "Low", Body = "Body", Status = "approved", LikeCount = 0, DislikeCount = 0, SubmittedAt = DateTimeOffset.UtcNow }
        };
        _mockRepo.Setup(r => r.GetApprovedAsync()).ReturnsAsync(letters);

        // Run multiple times - the high-score letter should appear most of the time
        var highCount = 0;
        for (int i = 0; i < 100; i++)
        {
            var result = await _service.GetNextLetterAsync(new List<string>());
            if (result!.Id == "high") highCount++;
        }

        // With weight 1000 vs 1, "high" should be selected ~99.9% of the time
        Assert.True(highCount > 90, $"Expected high-score letter to be selected most of the time, but was selected {highCount}/100 times");
    }

    [Fact]
    public async Task SubmitLetter_CreatesPendingLetter_WithCorrectFields()
    {
        Letter? createdLetter = null;
        _mockRepo.Setup(r => r.CreateAsync(It.IsAny<Letter>()))
            .Callback<Letter>(l => createdLetter = l)
            .Returns(Task.CompletedTask);

        await _service.SubmitLetterAsync(new SubmitLetterRequest("Title", "Body", "test@example.com"));

        Assert.NotNull(createdLetter);
        Assert.Equal("Title", createdLetter.Title);
        Assert.Equal("Body", createdLetter.Body);
        Assert.Equal("test@example.com", createdLetter.Email);
        Assert.Equal("pending", createdLetter.Status);
        Assert.False(string.IsNullOrEmpty(createdLetter.Id));
    }

    [Fact]
    public async Task SubmitLetter_ThrowsOnMissingTitle()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.SubmitLetterAsync(new SubmitLetterRequest("", "Body", null)));
    }

    [Fact]
    public async Task SubmitLetter_ThrowsOnMissingBody()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.SubmitLetterAsync(new SubmitLetterRequest("Title", "", null)));
    }

    [Fact]
    public async Task SubmitLetter_ThrowsOnTitleExceeding200Chars()
    {
        var longTitle = new string('a', 201);
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.SubmitLetterAsync(new SubmitLetterRequest(longTitle, "Body", null)));
    }

    [Fact]
    public async Task SubmitLetter_ThrowsOnBodyExceeding5000Chars()
    {
        var longBody = new string('a', 5001);
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.SubmitLetterAsync(new SubmitLetterRequest("Title", longBody, null)));
    }

    [Fact]
    public async Task SubmitLetter_ThrowsOnInvalidEmailFormat()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.SubmitLetterAsync(new SubmitLetterRequest("Title", "Body", "not-an-email")));
    }

    [Fact]
    public async Task SubmitFeedback_CallsIncrementLikeCount_ForLike()
    {
        await _service.SubmitFeedbackAsync("letter-1", "like");

        _mockRepo.Verify(r => r.IncrementLikeCountAsync("letter-1"), Times.Once);
    }

    [Fact]
    public async Task SubmitFeedback_CallsIncrementDislikeCount_ForDislike()
    {
        await _service.SubmitFeedbackAsync("letter-1", "dislike");

        _mockRepo.Verify(r => r.IncrementDislikeCountAsync("letter-1"), Times.Once);
    }

    [Fact]
    public async Task SubmitFeedback_ThrowsOnInvalidFeedbackType()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.SubmitFeedbackAsync("letter-1", "invalid"));
    }

    [Fact]
    public async Task ApproveLetter_CallsMoveToStatus_WithCorrectParameters()
    {
        await _service.ApproveLetterAsync("letter-1", "admin");

        _mockRepo.Verify(r => r.MoveToStatusAsync("letter-1", "pending", "approved", "admin"), Times.Once);
    }

    [Fact]
    public async Task RejectLetter_CallsMoveToStatus_WithCorrectParameters()
    {
        await _service.RejectLetterAsync("letter-1", "admin");

        _mockRepo.Verify(r => r.MoveToStatusAsync("letter-1", "pending", "rejected", "admin"), Times.Once);
    }
}
