namespace PromptProcessing.Application.Abstractions.AI;

public interface ITextGenerationService
{
    Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken);
}
