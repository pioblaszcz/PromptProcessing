using MassTransit;
using PromptProcessing.Application.Abstractions.Messaging;
using PromptProcessing.Contracts.PromptJobs;

namespace PromptProcessing.Infrastructure.Messaging;

public class PromptJobQueue(IPublishEndpoint publishEndpoint) : IPromptJobQueue
{
    public Task EnqueueAsync(Guid promptJobId, CancellationToken cancellationToken)
    {
        return publishEndpoint.Publish(new ProcessPromptJob(promptJobId), cancellationToken);
    }
}