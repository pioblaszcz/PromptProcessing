namespace PromptProcessing.Application.Abstractions.Messaging;

public interface IPromptJobQueue
{
    Task EnqueueAsync(Guid promptJobId, CancellationToken cancellationToken);
}