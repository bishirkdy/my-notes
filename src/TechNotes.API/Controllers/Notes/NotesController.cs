using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechNotes.Application.DTOs.Notes;
using TechNotes.Application.Features.Notes.Commands.CreateNote;
using TechNotes.Application.Features.Notes.Commands.DeleteNote;
using TechNotes.Application.Features.Notes.Commands.SaveReadingProgress;
using TechNotes.Application.Features.Notes.Commands.UpdateNote;
using TechNotes.Application.Features.Notes.Commands.UpdateNoteOrganization;
using TechNotes.Application.Features.Notes.Queries.GetMyNotes;
using TechNotes.Application.Features.Notes.Queries.GetNote;
using TechNotes.Application.Features.Notes.Queries.GetPublicNotes;
using TechNotes.Application.Features.Notes.Queries.GetSharedNotes;
using TechNotes.Application.Features.Notes.Queries.SearchNotes;
using TechNotes.API.Authorization;
using TechNotes.Domain.Constants;

namespace TechNotes.API.Controllers.Notes
{
    [ApiController]
    [Route("api/v1/notes")]
    public sealed class NotesController(IMediator mediator) : ControllerBase
    {
        [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
        [HttpPost]
        [ProducesResponseType<NoteResponse>(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Create(
            [FromBody] CreateNoteRequest request,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new CreateNoteCommand(GetCurrentUserId(), request),
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, result);
        }

        [AllowAnonymous]
        [HttpGet("{id:guid}")]
        [ProducesResponseType<NoteResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            Guid? userId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : null;
            var result = await mediator.Send(new GetNoteQuery(id, userId), cancellationToken);

            if (result is null)
            {
                return NotFound(new { message = "Note not found or you do not have access to it." });
            }

            return Ok(result);
        }

        [Authorize]
        [HttpGet("my")]
        [ProducesResponseType<IReadOnlyList<NoteResponse>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyNotes(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(new GetMyNotesQuery(GetCurrentUserId()), cancellationToken);
            return Ok(result);
        }

        [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
        [HttpPut("{id:guid}")]
        [ProducesResponseType<NoteResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateNoteRequest request,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new UpdateNoteCommand(id, GetCurrentUserId(), IsAdmin(), request),
                cancellationToken);

            if (result is null)
            {
                return NotFound(new { message = "Note not found or you are not the owner." });
            }

            return Ok(result);
        }

        [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
        [HttpPatch("{id:guid}/organization")]
        [ProducesResponseType<NoteResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> UpdateOrganization(
            Guid id,
            [FromBody] UpdateNoteOrganizationRequest request,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new UpdateNoteOrganizationCommand(id, GetCurrentUserId(), IsAdmin(), request),
                cancellationToken);

            if (result is null)
            {
                return NotFound(new { message = "Note not found or you are not the owner." });
            }

            return Ok(result);
        }

        [Authorize(Policy = AuthorizationPolicies.AdminOnly)]
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var deleted = await mediator.Send(
                new DeleteNoteCommand(id, GetCurrentUserId(), IsAdmin()),
                cancellationToken);

            return deleted
                ? NoContent()
                : NotFound(new { message = "Note not found or you are not the owner." });
        }

        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType<IReadOnlyList<NoteResponse>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPublicNotes(CancellationToken cancellationToken)
        {
            Guid? userId = User.Identity?.IsAuthenticated == true ? GetCurrentUserId() : null;
            var result = await mediator.Send(new GetPublicNotesQuery(userId), cancellationToken);
            return Ok(result);
        }

        [Authorize]
        [HttpPut("{id:guid}/progress")]
        [ProducesResponseType<ReadingProgressResponse>(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> SaveReadingProgress(
            Guid id,
            [FromBody] SaveReadingProgressRequest request,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new SaveReadingProgressCommand(id, GetCurrentUserId(), request),
                cancellationToken);

            return result is null
                ? NotFound(new { message = "Note not found or you do not have access to it." })
                : Ok(result);
        }

        [Authorize]
        [HttpGet("shared")]
        [ProducesResponseType<IReadOnlyList<NoteResponse>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetSharedNotes(CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new GetSharedNotesQuery(GetCurrentUserId()),
                cancellationToken);

            return Ok(result);
        }

        [Authorize]
        [HttpGet("search")]
        [ProducesResponseType<IReadOnlyList<NoteResponse>>(StatusCodes.Status200OK)]
        public async Task<IActionResult> Search(
            [FromQuery] string? searchTerm,
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new SearchNotesQuery(GetCurrentUserId(), searchTerm ?? string.Empty),
                cancellationToken);

            return Ok(result);
        }

        private bool IsAdmin() => User.IsInRole(RoleNames.Admin);

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
}
