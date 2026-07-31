using PromptProcessing.Domain.PromptJobs;

namespace PromptProcessing.Application.PromptJobs.GetPromptJobs;

public record PromptJobDto(
    Guid Id,
    string Content,
    PromptJobStatus Status,
    string? Result,
    string? ErrorMessage,
    DateTime CreatedAtUtc,
    DateTime? ProcessingStartedAtUtc,
    DateTime? CompletedAtUtc,
    int AttemptCount);
