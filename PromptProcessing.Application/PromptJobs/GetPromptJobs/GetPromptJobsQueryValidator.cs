using FluentValidation;
using PromptProcessing.Application.Common.Pagination;

namespace PromptProcessing.Application.PromptJobs.GetPromptJobs;

public class GetPromptJobsQueryValidator : AbstractValidator<GetPromptJobsQuery>
{
    public GetPromptJobsQueryValidator()
    {
        RuleFor(x => x.Take)
            .InclusiveBetween(1, PromptJobsPagination.MaximumPageSize);

        RuleFor(x => x)
            .Must(x => x.CreatedBeforeUtc.HasValue == x.Id.HasValue)
            .WithMessage("Cursor must include both creation time and ID.");
    }
}
