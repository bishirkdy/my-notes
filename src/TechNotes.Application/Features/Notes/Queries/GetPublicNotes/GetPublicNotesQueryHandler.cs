using MediatR;
using TechNotes.Application.DTOs.Notes;
using TechNotes.Application.Interfaces.Repositories;

namespace TechNotes.Application.Features.Notes.Queries.GetPublicNotes
{
    // Handles retrieval of publicly visible notes.
    public sealed class GetPublicNotesQueryHandler(
        INoteRepository noteRepository)
        : IRequestHandler<GetPublicNotesQuery, IReadOnlyList<NoteResponse>>
    {
        // Retrieves all public notes.
        public async Task<IReadOnlyList<NoteResponse>> Handle(GetPublicNotesQuery query, CancellationToken cancellationToken)
        {
            var notes = await noteRepository.GetPublicNotesAsync(cancellationToken);
            return notes
                .Select(MapToResponse)
                .ToList();
        }

        // Maps a note entity to its response DTO.
        private static NoteResponse MapToResponse(
            Domain.Entities.Notes.Note note)
        {
            return new NoteResponse
            {
                Id = note.Id,
                UserId = note.UserId,
                Title = note.Title,
                Status = note.Status,
                Visibility = note.Visibility,
                FolderId = note.FolderId,
                CategoryId = note.CategoryId
            };
        }
    }
}
