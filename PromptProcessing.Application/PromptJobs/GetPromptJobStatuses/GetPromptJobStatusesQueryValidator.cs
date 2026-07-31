using FluentValidation;
using PromptProcessing.Application.Common.Pagination;

namespace PromptProcessing.Application.PromptJobs.GetPromptJobStatuses;

public class GetPromptJobStatusesQueryValidator : AbstractValidator<GetPromptJobStatusesQuery>
{
    public GetPromptJobStatusesQueryValidator()
    {
        RuleFor(x => x.Ids)
            .NotEmpty()
            .Must(ids => ids.Count <= PromptJobsPagination.MaximumPageSize)
            .WithMessage($"Provide between 1 and {PromptJobsPagination.MaximumPageSize} job IDs.");
    }
}
