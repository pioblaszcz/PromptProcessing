using OllamaSharp;
using OllamaSharp.Models;
using PromptProcessing.Application.Abstractions.AI;
using System.Net;
using System.Text;

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

        try
        {
            await foreach (var response in ollamaApiClient.GenerateAsync(request, cancellationToken))
            {
                if (!string.IsNullOrEmpty(response?.Response))
                    result.Append(response.Response);
            }
        }
        catch (HttpRequestException exception) when (IsTransient(exception))
        {
            throw new TransientTextGenerationException(
                "The text generation service is temporarily unavailable.",
                exception);
        }
        catch (TimeoutException exception)
        {
            throw new TransientTextGenerationException(
                "The text generation service timed out.",
                exception);
        }
        catch (OperationCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            throw new TransientTextGenerationException(
                "The text generation service timed out.",
                exception);
        }

        if (result.Length == 0)
            throw new InvalidOperationException("Ollama returned an empty response.");

        return result.ToString();
    }

    private static bool IsTransient(HttpRequestException exception)
    {
        if (!exception.StatusCode.HasValue)
            return true;

        var statusCode = exception.StatusCode.Value;

        return statusCode is HttpStatusCode.RequestTimeout
            or HttpStatusCode.TooManyRequests
            || (int)statusCode >= 500;
    }
}
