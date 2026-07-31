using MediatR;
using Microsoft.AspNetCore.Mvc;
using PromptProcessing.Api.Contracts.PromptJobs;
using PromptProcessing.Application.PromptJobs.CreatePromptJobs;
using PromptProcessing.Application.PromptJobs.GetPromptJobs;

namespace PromptProcessing.Api.Controllers;

[ApiController]
[Route("api/prompt-jobs")]
public class PromptJobsController(ISender sender) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(IReadOnlyCollection<CreatedPromptJobDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyCollection<CreatedPromptJobDto>>> Create([FromBody] CreatePromptJobsRequest request, CancellationToken cancellationToken)
    {
        var command = new CreatePromptJobsCommand(request.Prompts ?? []);
        var promptJobs = await sender.Send(command, cancellationToken);

        return StatusCode(StatusCodes.Status201Created, promptJobs);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<PromptJobDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<PromptJobDto>>> GetAll(CancellationToken cancellationToken)
    {
        var promptJobs = await sender.Send(new GetPromptJobsQuery(), cancellationToken);
        return Ok(promptJobs);
    }
}