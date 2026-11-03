using MediatR;
using TechNotes.Application.DTOs.Notes;

namespace TechNotes.Application.Features.Notes.Commands.UpdateNoteOrganization;

public sealed record UpdateNoteOrganizationCommand(
    Guid NoteId,
    Guid UserId,
    bool IsAdmin,
    UpdateNoteOrganizationRequest Request) : IRequest<NoteResponse?>;
