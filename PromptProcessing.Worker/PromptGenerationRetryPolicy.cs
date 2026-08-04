namespace PromptProcessing.Worker;

public static class PromptGenerationRetryPolicy
{
    public const int RetryCount = 2;
    public static readonly TimeSpan[] RetryIntervals = [TimeSpan.FromSeconds(2), TimeSpan.FromSeconds(5)];

    public static bool IsFinalAttempt(int retryAttempt) => retryAttempt >= RetryCount;
}
