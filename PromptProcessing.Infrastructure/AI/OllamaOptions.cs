namespace PromptProcessing.Infrastructure.AI;

public class OllamaOptions
{
    public const string SectionName = "Ollama";

    public string BaseUrl { get; init; } = string.Empty;
    public string Model { get; init; } = string.Empty;
}
