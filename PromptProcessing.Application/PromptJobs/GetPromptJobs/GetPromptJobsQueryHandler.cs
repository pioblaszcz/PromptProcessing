using MediatR;
using PromptProcessing.Application.Abstractions.Persistence;

namespace PromptProcessing.Application.PromptJobs.GetPromptJobs;

public class GetPromptJobsQueryHandler(IPromptJobRepository promptJobRepository) : IRequestHandler<GetPromptJobsQuery, PromptJobsPageDto>
{
    public async Task<PromptJobsPageDto> Handle(GetPromptJobsQuery request, CancellationToken cancellationToken)
    {
        var promptJobs = await promptJobRepository.GetPageAsync(request.Take + 1, request.CreatedBeforeUtc, request.Id, cancellationToken);
        var items = promptJobs
            .Take(request.Take)
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

        DateTime? nextCreatedBeforeUtc = promptJobs.Count > request.Take
            ? items.Last().CreatedAtUtc
            : null;
        Guid? nextId = promptJobs.Count > request.Take
            ? items.Last().Id
            : null;

        return new PromptJobsPageDto(items, nextCreatedBeforeUtc, nextId);
    }
}
