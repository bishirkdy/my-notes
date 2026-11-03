using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TechNotes.API.Execution;

namespace TechNotes.API.Controllers.Executions;

[ApiController]
[Route("api/v1/executions")]
[AllowAnonymous]
public sealed class ExecutionsController(CodeExecutionService execution) : ControllerBase
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
        var result = await execution.ExecuteAsync(request, cancellationToken);
        return Ok(result);
    }
}
