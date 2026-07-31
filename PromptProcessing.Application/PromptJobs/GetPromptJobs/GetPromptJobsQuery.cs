using MediatR;

namespace PromptProcessing.Application.PromptJobs.GetPromptJobs;

public record GetPromptJobsQuery : IRequest<IReadOnlyCollection<PromptJobDto>>;
