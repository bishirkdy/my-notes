using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TechNotes.Application.DTOs.Notes;
using TechNotes.Application.Features.Notes.Commands.CreateNote;
using TechNotes.Application.Features.Notes.Commands.DeleteNote;
using TechNotes.Application.Features.Notes.Commands.UpdateNote;
using TechNotes.Application.Features.Notes.Queries.GetMyNotes;
using TechNotes.Application.Features.Notes.Queries.GetNote;
using TechNotes.Application.Features.Notes.Queries.GetPublicNotes;
using TechNotes.Application.Features.Notes.Queries.SearchNotes;

namespace TechNotes.API.Controllers.Notes
{
    [Route("api/v1/notes")]
    public sealed class NotesController(IMediator mediator) : ControllerBase
    {
        // Creates a new note for the authenticated user.
        [Authorize]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create([FromBody] CreateNoteRequest request, CancellationToken cancellationToken)
        {
            // Gets the owner ID from the authenticated JWT.
            var userId = GetCurrentUserId();

            // Creates the CQRS command using the authenticated user.
            var command = new CreateNoteCommand(userId, request);

            // Sends the command to CreateNoteCommandHandler.
            var result = await mediator.Send(command, cancellationToken);

            // Returns the newly created note.
            return StatusCode(StatusCodes.Status201Created, result);
        }

        // Gets the authenticated user's ID from the JWT claims.
        private Guid GetCurrentUserId()
        {
            var value = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");

            if (!Guid.TryParse(value, out var userId))
            {
                throw new UnauthorizedAccessException("The authenticated user ID is invalid.");
            }

            return userId;
        }

        // Retrieves a note when the current user has permission to view it.
        [AllowAnonymous]
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id , CancellationToken cancellationToken)
        {
            // Anonymous visitors have no user ID.
            Guid? userId = null;

            // Extract the user ID when an authenticated user is making the request.
            if(User.Identity?.IsAuthenticated == true)
            {
                userId = GetCurrentUserId();
            }

            // Sends the read operation through MediatR.
            var result = await mediator.Send(new GetNoteQuery(id, userId) , cancellationToken);
            
            // Return 404 when the note doesn't exist or cannot be accessed.
            if(result is null)
            {
                return NotFound(new
                {
                    message = "Note not found or you do not have access to it."
                });
            }

            return Ok(result);
        }

        // Retrieves all notes owned by the authenticated user.
        [Authorize]
        [HttpGet("my")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyNotes(
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            var result = await mediator.Send(
                new GetMyNotesQuery(userId),
                cancellationToken);

            return Ok(result);
        }

        // Updates the authenticated user's note.
        [Authorize]
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateNoteRequest request,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            var result = await mediator.Send(
                new UpdateNoteCommand(id, userId, request),
                cancellationToken);

            if (result is null)
            {
                return NotFound(new
                {
                    message = "Note not found or you are not the owner."
                });
            }
            return Ok(result);
        }

        // Deletes the authenticated user's note.
        [Authorize]
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(
            Guid id,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            var deleted = await mediator.Send(
                new DeleteNoteCommand(id, userId),
                cancellationToken);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Note not found or you are not the owner."
                });
            }
            return NoContent();
        }

        // Retrieves notes that are publicly visible.
        [AllowAnonymous]
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPublicNotes(
            CancellationToken cancellationToken)
        {
            var result = await mediator.Send(
                new GetPublicNotesQuery(),
                cancellationToken);

            return Ok(result);
        }

        // Searches the authenticated user's notes by title.
        [Authorize]
        [HttpGet("search")]
        public async Task<IActionResult> Search(
            [FromQuery] string searchTerm,
            CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();

            var result = await mediator.Send(
                new SearchNotesQuery(userId, searchTerm),
                cancellationToken);

            return Ok(result);
        }
    }
}
