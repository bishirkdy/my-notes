using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TechNotes.API.Authorization;
using TechNotes.Application.DTOs.Categories;
using TechNotes.Application.Features.Categories.Commands.CreateCategory;
using TechNotes.Application.Features.Categories.Commands.DeleteCategory;
using TechNotes.Application.Features.Categories.Commands.UpdateCategory;
using TechNotes.Application.Features.Categories.Queries.GetCategories;
using TechNotes.Application.Features.Categories.Queries.GetCategoryTree;

namespace TechNotes.API.Controllers.Categories;

[ApiController]
[Authorize(Policy = AuthorizationPolicies.AdminOnly)]
[Route("api/v1/categories")]
public sealed class CategoriesController(IMediator mediator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType<CategoryResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new CreateCategoryCommand(GetCurrentUserId(), request),
            cancellationToken);

        if (result is null)
        {
            return Conflict(new { message = "A category with this name already exists." });
        }

        return StatusCode(StatusCodes.Status201Created, result);
    }

    [HttpGet]
    [ProducesResponseType<IReadOnlyList<CategoryResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetCategoriesQuery(GetCurrentUserId()),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("tree")]
    [ProducesResponseType<IReadOnlyList<CategoryTreeResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetTree(CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new GetCategoryTreeQuery(GetCurrentUserId()),
            cancellationToken);

        return Ok(result);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType<CategoryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateCategoryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await mediator.Send(
            new UpdateCategoryCommand(id, GetCurrentUserId(), request),
            cancellationToken);

        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        var deleted = await mediator.Send(
            new DeleteCategoryCommand(id, GetCurrentUserId()),
            cancellationToken);

        return deleted ? NoContent() : NotFound();
    }

    private Guid GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

        if (!Guid.TryParse(value, out var userId))
        {
            throw new UnauthorizedAccessException("The authenticated user ID is invalid.");
        }

        return userId;
    }
}
