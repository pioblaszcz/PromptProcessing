using PromptProcessing.Application.Common.Pagination;
using PromptProcessing.Application.PromptJobs.CreatePromptJobs;
using PromptProcessing.Application.PromptJobs.GetPromptJobs;
using PromptProcessing.Application.PromptJobs.GetPromptJobStatuses;
using PromptProcessing.Domain.PromptJobs;

namespace PromptProcessing.Application.Tests.PromptJobs.Validators;

public class PromptJobValidatorsTests
{
    [Fact]
    public async Task CreatePromptJobsCommandValidator_ValidPrompts_IsValid()
    {
        var validator = new CreatePromptJobsCommandValidator();

        var result = await validator.ValidateAsync(new CreatePromptJobsCommand(new[] { "First prompt", "Second prompt" }));

        Assert.True(result.IsValid);
    }

    [Fact]
    public async Task CreatePromptJobsCommandValidator_EmptyPrompts_IsInvalid()
    {
        var validator = new CreatePromptJobsCommandValidator();

        var result = await validator.ValidateAsync(new CreatePromptJobsCommand(Array.Empty<string>()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Prompts");
    }

    [Fact]
    public async Task CreatePromptJobsCommandValidator_WhitespacePrompt_IsInvalid()
    {
        var validator = new CreatePromptJobsCommandValidator();

        var result = await validator.ValidateAsync(new CreatePromptJobsCommand(new[] { "   " }));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Prompts[0]");
    }

    [Fact]
    public async Task CreatePromptJobsCommandValidator_OverlongPrompt_IsInvalid()
    {
        var validator = new CreatePromptJobsCommandValidator();
        var command = new CreatePromptJobsCommand(new[] { new string('a', PromptJob.MaxContentLength + 1) });

        var result = await validator.ValidateAsync(command);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Prompts[0]");
    }

    [Fact]
    public async Task GetPromptJobsQueryValidator_TakeOutsideRange_IsInvalid()
    {
        var validator = new GetPromptJobsQueryValidator();

        var result = await validator.ValidateAsync(new GetPromptJobsQuery(PromptJobsPagination.MaximumPageSize + 1, null, null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Take");
    }

    [Fact]
    public async Task GetPromptJobsQueryValidator_IncompleteCursor_IsInvalid()
    {
        var validator = new GetPromptJobsQueryValidator();

        var result = await validator.ValidateAsync(new GetPromptJobsQuery(PromptJobsPagination.DefaultPageSize, DateTime.UtcNow, null));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.ErrorMessage == "Cursor must include both creation time and ID.");
    }

    [Fact]
    public async Task GetPromptJobStatusesQueryValidator_EmptyIds_IsInvalid()
    {
        var validator = new GetPromptJobStatusesQueryValidator();

        var result = await validator.ValidateAsync(new GetPromptJobStatusesQuery(Array.Empty<Guid>()));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Ids");
    }

    [Fact]
    public async Task GetPromptJobStatusesQueryValidator_IdsExceedMaximum_IsInvalid()
    {
        var validator = new GetPromptJobStatusesQueryValidator();
        var ids = Enumerable.Range(0, PromptJobsPagination.MaximumPageSize + 1).Select(_ => Guid.NewGuid()).ToList();

        var result = await validator.ValidateAsync(new GetPromptJobStatusesQuery(ids));

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Ids");
    }
}
