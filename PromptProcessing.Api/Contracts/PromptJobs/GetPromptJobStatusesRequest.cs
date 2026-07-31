using PromptProcessing.Application.PromptJobs.GetPromptJobStatuses;

namespace PromptProcessing.Api.Contracts.PromptJobs;

public record GetPromptJobStatusesRequest(Guid[]? Ids)
{
    public GetPromptJobStatusesQuery ToQuery()
    {
        return new GetPromptJobStatusesQuery(Ids ?? Array.Empty<Guid>());
    }
}
