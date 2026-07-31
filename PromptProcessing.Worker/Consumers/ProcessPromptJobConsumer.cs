using MassTransit;
using Microsoft.Extensions.Logging;
using PromptProcessing.Application.Abstractions.Persistence;
using PromptProcessing.Contracts.PromptJobs;
using PromptProcessing.Domain.PromptJobs;

namespace PromptProcessing.Worker.Consumers;

public class ProcessPromptJobConsumer(IPromptJobRepository promptJobRepository, IUnitOfWork unitOfWork, ILogger<ProcessPromptJobConsumer> logger) : IConsumer<ProcessPromptJob>
{
    public async Task Consume(ConsumeContext<ProcessPromptJob> context)
    {
        var promptJob = await promptJobRepository.GetByIdAsync(context.Message.PromptJobId, context.CancellationToken);

        if (promptJob is null)
        {
            logger.LogWarning("Prompt job {PromptJobId} was not found.", context.Message.PromptJobId);
            return;
        }

        if (promptJob.Status != PromptJobStatus.Pending)
        {
            logger.LogInformation("Prompt job {PromptJobId} has status {Status} and will not be processed.", promptJob.Id, promptJob.Status);
            return;
        }

        promptJob.StartProcessing();
        await unitOfWork.SaveChangesAsync(context.CancellationToken);

        logger.LogInformation("Prompt job {PromptJobId} started processing.", promptJob.Id);
    }
}