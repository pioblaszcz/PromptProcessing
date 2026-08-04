namespace PromptProcessing.Application.Abstractions.AI;

public sealed class TransientTextGenerationException(string message, Exception innerException) : Exception(message, innerException);
