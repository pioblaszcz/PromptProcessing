namespace PromptProcessing.Domain.PromptJobs;

public class PromptJob
{
    public const int MaxContentLength = 10_000;
    private PromptJob() { }

    public Guid Id { get; private set; }
    public string Content { get; private set; } = string.Empty;
    public PromptJobStatus Status { get; private set; }
    public string? Result { get; private set; }
    public string? ErrorMessage { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime? ProcessingStartedAtUtc { get; private set; }
    public DateTime? CompletedAtUtc { get; private set; }
    public int AttemptCount { get; private set; }

    public static PromptJob Create(string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Prompt content cannot be empty.", nameof(content));

        var normalizedContent = content.Trim();

        if (normalizedContent.Length > MaxContentLength)
            throw new ArgumentException($"Prompt content cannot exceed {MaxContentLength} characters.", nameof(content));

        return new PromptJob
        {
            Id = Guid.NewGuid(),
            Content = normalizedContent,
            Status = PromptJobStatus.Pending,
            CreatedAtUtc = DateTime.UtcNow
        };
    }

    public void StartProcessing()
    {
        if (Status != PromptJobStatus.Pending)
            throw new InvalidOperationException($"Prompt job with status '{Status}' cannot start processing.");

        Status = PromptJobStatus.Processing;
        ProcessingStartedAtUtc = DateTime.UtcNow;
        AttemptCount++;
        ErrorMessage = null;
    }

    public void Complete(string result)
    {
        if (Status != PromptJobStatus.Processing)
            throw new InvalidOperationException($"Prompt job with status '{Status}' cannot be completed.");

        if (string.IsNullOrWhiteSpace(result))
            throw new ArgumentException("Prompt result cannot be empty.", nameof(result));

        Status = PromptJobStatus.Completed;
        Result = result;
        ErrorMessage = null;
        CompletedAtUtc = DateTime.UtcNow;
    }

    public void Fail(string errorMessage)
    {
        if (Status != PromptJobStatus.Processing)
            throw new InvalidOperationException($"Prompt job with status '{Status}' cannot fail.");

        if (string.IsNullOrWhiteSpace(errorMessage))
            throw new ArgumentException("Error message cannot be empty.", nameof(errorMessage));

        Status = PromptJobStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAtUtc = DateTime.UtcNow;
    }
}