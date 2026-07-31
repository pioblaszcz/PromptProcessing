namespace PromptProcessing.Api.Pagination;

public sealed class InvalidPromptJobsCursorException : Exception
{
    public InvalidPromptJobsCursorException() : base("Cursor is invalid.")
    {
    }

    public InvalidPromptJobsCursorException(Exception innerException) : base("Cursor is invalid.", innerException)
    {
    }
}