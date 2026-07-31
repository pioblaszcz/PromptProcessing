using PromptProcessing.Api.Pagination;
using PromptProcessing.Application.PromptJobs.GetPromptJobs;

namespace PromptProcessing.Api.Contracts.PromptJobs;

public record PromptJobsPageResponse(IReadOnlyCollection<PromptJobDto> Items, string? NextCursor)
{
    public static PromptJobsPageResponse From(PromptJobsPageDto page, PromptJobsCursorCodec cursorCodec)
    {
        PromptJobsCursor? cursor = page.NextCreatedBeforeUtc.HasValue && page.NextId.HasValue
            ? new(page.NextCreatedBeforeUtc.Value, page.NextId.Value)
            : null;

        return new(page.Items, cursorCodec.Encode(cursor));
    }
}