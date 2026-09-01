using MediatR;
using TechNotes.Application.DTOs.Notes;
using TechNotes.Application.Interfaces.Repositories;

namespace TechNotes.Application.Features.Notes.Queries.SearchNotes
{
    // Handles searching notes by title.
    public sealed class SearchNotesQueryHandler(
        INoteRepository noteRepository)
        : IRequestHandler<SearchNotesQuery, IReadOnlyList<NoteResponse>>
    {
        // Searches the authenticated user's notes.
        public async Task<IReadOnlyList<NoteResponse>> Handle(
            SearchNotesQuery query,
            CancellationToken cancellationToken)
        {
            var notes = await noteRepository.SearchAsync(
                query.UserId,
                query.SearchTerm,
                cancellationToken);

            return notes
                .Select(note => new NoteResponse
                {
                    Id = note.Id,
                    UserId = note.UserId,
                    Title = note.Title,
                    Status = note.Status,
                    Visibility = note.Visibility,
                    FolderId = note.FolderId,
                    CategoryId = note.CategoryId
                })
                .ToList();
        }
    }
}
