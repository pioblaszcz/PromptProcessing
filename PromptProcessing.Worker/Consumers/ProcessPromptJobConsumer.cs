using MassTransit;
using Microsoft.Extensions.Logging;
using PromptProcessing.Application.Abstractions.AI;
using PromptProcessing.Application.Abstractions.Persistence;
using PromptProcessing.Contracts.PromptJobs;
using PromptProcessing.Domain.PromptJobs;

namespace PromptProcessing.Worker.Consumers;

public class ProcessPromptJobConsumer(
    IPromptJobRepository promptJobRepository,
    IUnitOfWork unitOfWork,
    ITextGenerationService textGenerationService,
    ILogger<ProcessPromptJobConsumer> logger) : IConsumer<ProcessPromptJob>
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

        try
        {
            var result = await textGenerationService.GenerateAsync(promptJob.Content, context.CancellationToken);

            promptJob.Complete(result);
            await unitOfWork.SaveChangesAsync(context.CancellationToken);

            logger.LogInformation("Prompt job {PromptJobId} completed processing.", promptJob.Id);
        }
        catch (OperationCanceledException) when (context.CancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Prompt job {PromptJobId} failed processing.", promptJob.Id);

            promptJob.Fail("Prompt processing failed. Please try again later.");
            await unitOfWork.SaveChangesAsync(context.CancellationToken);
        }
    }
}
