using PromptProcessing.Api.Pagination;
using PromptProcessing.Application.Common.Pagination;
using PromptProcessing.Application.PromptJobs.GetPromptJobs;

namespace PromptProcessing.Api.Contracts.PromptJobs;

public record GetPromptJobsRequest(int? Take = null, string? Cursor = null)
{
    public GetPromptJobsQuery ToQuery(PromptJobsCursorCodec cursorCodec)
    {
        var cursor = cursorCodec.Decode(Cursor);

        return new GetPromptJobsQuery(Take ?? PromptJobsPagination.DefaultPageSize, cursor?.CreatedAtUtc, cursor?.Id);
    }
}