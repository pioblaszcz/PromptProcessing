using MediatR;
using PromptProcessing.Application.Abstractions.Messaging;
using PromptProcessing.Application.Abstractions.Persistence;
using PromptProcessing.Domain.PromptJobs;

namespace PromptProcessing.Application.PromptJobs.CreatePromptJobs;

public class CreatePromptJobsCommandHandler(
    IPromptJobRepository promptJobRepository,
    IPromptJobQueue promptJobQueue,
    IUnitOfWork unitOfWork) : IRequestHandler<CreatePromptJobsCommand, IReadOnlyCollection<CreatedPromptJobDto>>
{
    public async Task<IReadOnlyCollection<CreatedPromptJobDto>> Handle(CreatePromptJobsCommand request, CancellationToken cancellationToken)
    {
        var promptJobs = request.Prompts.Select(PromptJob.Create).ToList();

        await promptJobRepository.AddRangeAsync(promptJobs, cancellationToken);

        foreach (var promptJob in promptJobs)
            await promptJobQueue.EnqueueAsync(promptJob.Id, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return promptJobs.Select(x => new CreatedPromptJobDto(x.Id, x.Content)).ToList();
    }
}
