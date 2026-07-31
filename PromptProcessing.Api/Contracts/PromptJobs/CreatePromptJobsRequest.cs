namespace PromptProcessing.Api.Contracts.PromptJobs;

public record CreatePromptJobsRequest(IReadOnlyCollection<string>? Prompts);
