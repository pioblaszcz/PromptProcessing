namespace PromptProcessing.Application.PromptJobs.GetPromptJobs;

public record PromptJobsPageDto(IReadOnlyCollection<PromptJobDto> Items, DateTime? NextCreatedBeforeUtc, Guid? NextId);
