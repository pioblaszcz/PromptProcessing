using System.Text;
using OllamaSharp;
using OllamaSharp.Models;
using PromptProcessing.Application.Abstractions.AI;

namespace PromptProcessing.Infrastructure.AI;

public class OllamaTextGenerationService(OllamaApiClient ollamaApiClient, OllamaOptions options) : ITextGenerationService
{
    public async Task<string> GenerateAsync(string prompt, CancellationToken cancellationToken)
    {
        var result = new StringBuilder();

        var request = new GenerateRequest
        {
            Model = options.Model,
            Prompt = prompt
        };

        await foreach (var response in ollamaApiClient.GenerateAsync(request, cancellationToken))
        {
            var responseText = response?.Response;

            if (!string.IsNullOrEmpty(responseText))
                result.Append(responseText);
        }

        if (result.Length == 0)
            throw new InvalidOperationException("Ollama returned an empty response.");

        return result.ToString();
    }
}
