using PromptProcessing.Domain.PromptJobs;

namespace PromptProcessing.Domain.Tests.PromptJobs;

public class PromptJobTests
{
    [Fact]
    public void Create_ValidContent_InitializesPendingJob()
    {
        var before = DateTime.UtcNow;

        var promptJob = PromptJob.Create("Explain message queues.");

        var after = DateTime.UtcNow;
        Assert.NotEqual(Guid.Empty, promptJob.Id);
        Assert.Equal("Explain message queues.", promptJob.Content);
        Assert.Equal(PromptJobStatus.Pending, promptJob.Status);
        Assert.InRange(promptJob.CreatedAtUtc, before, after);
        Assert.Equal(0, promptJob.AttemptCount);
        Assert.Null(promptJob.Result);
        Assert.Null(promptJob.ErrorMessage);
        Assert.Null(promptJob.ProcessingStartedAtUtc);
        Assert.Null(promptJob.CompletedAtUtc);
    }

    [Fact]
    public void Create_ContentWithSurroundingWhitespace_TrimsContent()
    {
        var promptJob = PromptJob.Create("  Explain message queues.  ");

        Assert.Equal("Explain message queues.", promptJob.Content);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_InvalidContent_ThrowsArgumentException(string? content)
    {
        Assert.Throws<ArgumentException>(() => PromptJob.Create(content!));
    }

    [Fact]
    public void Create_ContentExceedingMaximumLength_ThrowsArgumentException()
    {
        var content = new string('a', PromptJob.MaxContentLength + 1);

        Assert.Throws<ArgumentException>(() => PromptJob.Create(content));
    }

    [Fact]
    public void StartProcessing_PendingJob_UpdatesProcessingState()
    {
        var promptJob = PromptJob.Create("Explain message queues.");
        var before = DateTime.UtcNow;

        promptJob.StartProcessing();

        var after = DateTime.UtcNow;
        Assert.Equal(PromptJobStatus.Processing, promptJob.Status);
        Assert.InRange(promptJob.ProcessingStartedAtUtc!.Value, before, after);
        Assert.Equal(1, promptJob.AttemptCount);
        Assert.Null(promptJob.Result);
        Assert.Null(promptJob.ErrorMessage);
        Assert.Null(promptJob.CompletedAtUtc);
    }

    [Fact]
    public void StartProcessing_ProcessingJob_ThrowsInvalidOperationException()
    {
        var promptJob = PromptJob.Create("Explain message queues.");
        promptJob.StartProcessing();

        Assert.Throws<InvalidOperationException>(() => promptJob.StartProcessing());
    }

    [Fact]
    public void Complete_ProcessingJob_StoresResultAndCompletesJob()
    {
        var promptJob = CreateProcessingJob();
        var before = DateTime.UtcNow;

        promptJob.Complete("A message queue decouples producers from consumers.");

        var after = DateTime.UtcNow;
        Assert.Equal(PromptJobStatus.Completed, promptJob.Status);
        Assert.Equal("A message queue decouples producers from consumers.", promptJob.Result);
        Assert.Null(promptJob.ErrorMessage);
        Assert.InRange(promptJob.CompletedAtUtc!.Value, before, after);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Complete_EmptyResult_ThrowsArgumentException(string result)
    {
        var promptJob = CreateProcessingJob();

        Assert.Throws<ArgumentException>(() => promptJob.Complete(result));
    }

    [Fact]
    public void Complete_PendingJob_ThrowsInvalidOperationException()
    {
        var promptJob = PromptJob.Create("Explain message queues.");

        Assert.Throws<InvalidOperationException>(() => promptJob.Complete("Result"));
    }

    [Fact]
    public void Complete_CompletedJob_ThrowsInvalidOperationException()
    {
        var promptJob = CreateProcessingJob();
        promptJob.Complete("First result");

        Assert.Throws<InvalidOperationException>(() => promptJob.Complete("Second result"));
    }

    [Fact]
    public void Fail_ProcessingJob_StoresErrorAndFailsJob()
    {
        var promptJob = CreateProcessingJob();
        var before = DateTime.UtcNow;

        promptJob.Fail("The model is unavailable.");

        var after = DateTime.UtcNow;
        Assert.Equal(PromptJobStatus.Failed, promptJob.Status);
        Assert.Equal("The model is unavailable.", promptJob.ErrorMessage);
        Assert.Null(promptJob.Result);
        Assert.InRange(promptJob.CompletedAtUtc!.Value, before, after);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Fail_EmptyErrorMessage_ThrowsArgumentException(string errorMessage)
    {
        var promptJob = CreateProcessingJob();

        Assert.Throws<ArgumentException>(() => promptJob.Fail(errorMessage));
    }

    [Fact]
    public void Fail_PendingJob_ThrowsInvalidOperationException()
    {
        var promptJob = PromptJob.Create("Explain message queues.");

        Assert.Throws<InvalidOperationException>(() => promptJob.Fail("The model is unavailable."));
    }

    [Fact]
    public void Fail_CompletedJob_ThrowsInvalidOperationException()
    {
        var promptJob = CreateProcessingJob();
        promptJob.Complete("Result");

        Assert.Throws<InvalidOperationException>(() => promptJob.Fail("The model is unavailable."));
    }

    private static PromptJob CreateProcessingJob()
    {
        var promptJob = PromptJob.Create("Explain message queues.");
        promptJob.StartProcessing();

        return promptJob;
    }
}
