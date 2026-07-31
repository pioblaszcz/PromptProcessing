using MediatR;
using PromptProcessing.Application.Abstractions.Persistence;
using PromptProcessing.Application.PromptJobs.GetPromptJobs;

namespace PromptProcessing.Application.PromptJobs.GetPromptJobStatuses;

public class GetPromptJobStatusesQueryHandler(IPromptJobRepository promptJobRepository) : IRequestHandler<GetPromptJobStatusesQuery, IReadOnlyCollection<PromptJobDto>>
{
    public async Task<IReadOnlyCollection<PromptJobDto>> Handle(GetPromptJobStatusesQuery request, CancellationToken cancellationToken)
    {
        var promptJobs = await promptJobRepository.GetByIdsAsync(request.Ids, cancellationToken);

        return promptJobs
            .Select(x => new PromptJobDto(
                x.Id,
                x.Content,
                x.Status,
                x.Result,
                x.ErrorMessage,
                x.CreatedAtUtc,
                x.ProcessingStartedAtUtc,
                x.CompletedAtUtc,
                x.AttemptCount))
            .ToList();
    }
}
