using FluentValidation;
using PromptProcessing.Domain.PromptJobs;

namespace PromptProcessing.Application.PromptJobs.CreatePromptJobs;

public class CreatePromptJobsCommandValidator : AbstractValidator<CreatePromptJobsCommand>
{
    public CreatePromptJobsCommandValidator()
    {
        RuleFor(x => x.Prompts)
            .NotEmpty()
            .WithMessage("At least one prompt is required.");

        RuleForEach(x => x.Prompts)
            .NotEmpty()
            .WithMessage("Prompt content cannot be empty.")
            .MaximumLength(PromptJob.MaxContentLength)
            .WithMessage($"Prompt content cannot exceed {PromptJob.MaxContentLength} characters.");
    }
}