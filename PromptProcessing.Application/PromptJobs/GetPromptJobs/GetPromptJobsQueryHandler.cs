using MediatR;
using PromptProcessing.Application.Abstractions.Persistence;

namespace PromptProcessing.Application.PromptJobs.GetPromptJobs;

public class GetPromptJobsQueryHandler(IPromptJobRepository promptJobRepository) : IRequestHandler<GetPromptJobsQuery, IReadOnlyCollection<PromptJobDto>>
{
    public async Task<IReadOnlyCollection<PromptJobDto>> Handle(GetPromptJobsQuery request, CancellationToken cancellationToken)
    {
        var promptJobs = await promptJobRepository.GetAllAsync(cancellationToken);

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
