using MediatR;
using PromptProcessing.Application.PromptJobs.GetPromptJobs;

namespace PromptProcessing.Application.PromptJobs.GetPromptJobStatuses;

public record GetPromptJobStatusesQuery(IReadOnlyCollection<Guid> Ids) : IRequest<IReadOnlyCollection<PromptJobDto>>;
