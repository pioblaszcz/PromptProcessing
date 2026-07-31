using MediatR;
using Microsoft.AspNetCore.Mvc;
using PromptProcessing.Api.Contracts.PromptJobs;
using PromptProcessing.Api.Pagination;
using PromptProcessing.Application.PromptJobs.CreatePromptJobs;
using PromptProcessing.Application.PromptJobs.GetPromptJobs;

namespace PromptProcessing.Api.Controllers;

[ApiController]
[Route("api/prompt-jobs")]
public class PromptJobsController(ISender sender, PromptJobsCursorCodec cursorCodec) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(IReadOnlyCollection<CreatedPromptJobDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyCollection<CreatedPromptJobDto>>> Create([FromBody] CreatePromptJobsRequest request, CancellationToken cancellationToken)
    {
        var promptJobs = await sender.Send(new CreatePromptJobsCommand(request.Prompts ?? []), cancellationToken);

        return StatusCode(StatusCodes.Status201Created, promptJobs);
    }

    [HttpGet]
    [ProducesResponseType(typeof(PromptJobsPageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PromptJobsPageResponse>> GetPage([FromQuery] GetPromptJobsRequest request, CancellationToken cancellationToken)
    {
        var page = await sender.Send(request.ToQuery(cursorCodec), cancellationToken);

        return Ok(PromptJobsPageResponse.From(page, cursorCodec));
    }

    [HttpGet("status")]
    [ProducesResponseType(typeof(IReadOnlyCollection<PromptJobDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<IReadOnlyCollection<PromptJobDto>>> GetStatuses([FromQuery] GetPromptJobStatusesRequest request, CancellationToken cancellationToken)
    {
        var promptJobs = await sender.Send(request.ToQuery(), cancellationToken);

        return Ok(promptJobs);
    }
}
