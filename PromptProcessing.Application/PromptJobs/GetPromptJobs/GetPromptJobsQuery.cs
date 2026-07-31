using MediatR;

namespace PromptProcessing.Application.PromptJobs.GetPromptJobs;

public record GetPromptJobsQuery(int Take, DateTime? CreatedBeforeUtc, Guid? Id) : IRequest<PromptJobsPageDto>;
