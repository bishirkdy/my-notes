using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using TechNotes.API.Execution;

namespace TechNotes.API.Controllers.Executions;

[ApiController]
[Route("api/v1/executions")]
[AllowAnonymous]
public sealed class ExecutionsController(
    CodeExecutionService execution,
    IOptions<ExecutionOptions> options) : ControllerBase
{
    [HttpGet("languages")]
    public IActionResult GetLanguages()
    {
        return Ok(ExecutionLanguageRegistry.All);
    }

    [HttpPost]
    [EnableRateLimiting("executions")]
    [RequestSizeLimit(131_072)]
    [ProducesResponseType<ExecuteCodeResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<IActionResult> Execute(
        [FromBody] ExecuteCodeRequest request,
        CancellationToken cancellationToken)
    {
        if (!options.Value.Enabled)
        {
            return Ok(new ExecuteCodeResponse(
                false,
                "run",
                string.Empty,
                "Code execution is disabled on this deployment.",
                [],
                0,
                request.Language,
                null));
        }

        var result = await execution.ExecuteAsync(request, cancellationToken);
        return Ok(result);
    }
}
