using MediatR;

namespace PromptProcessing.Application.PromptJobs.CreatePromptJobs;

public record CreatePromptJobsCommand(IReadOnlyCollection<string> Prompts) : IRequest<IReadOnlyCollection<CreatedPromptJobDto>>;