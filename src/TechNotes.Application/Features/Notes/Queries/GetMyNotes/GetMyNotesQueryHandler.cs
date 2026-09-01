

using MediatR;
using TechNotes.Application.DTOs.Notes;
using TechNotes.Application.Interfaces.Repositories;

namespace TechNotes.Application.Features.Notes.Queries.GetMyNotes
{
    // Handles retrieval of notes owned by the authenticated user.
    public sealed class GetMyNotesQueryHandler(
        INoteRepository noteRepository)
        : IRequestHandler<GetMyNotesQuery, IReadOnlyList<NoteResponse>>
    {
        // Retrieves the user's notes and maps them to response DTOs.
        public async Task<IReadOnlyList<NoteResponse>> Handle(
            GetMyNotesQuery query,
            CancellationToken cancellationToken)
        {
            var notes = await noteRepository.GetByUserIdAsync(
                query.UserId,
                cancellationToken);

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
                CategoryId = note.CategoryId,

                Sections = note.Sections
                    .Where(x => x.ParentSectionId == null)
                    .OrderBy(x => x.SortOrder)
                    .Select(MapSection)
                    .ToList()
            };
        }

        // Recursively maps sections and their content.
        private static NoteSectionResponse MapSection(
            Domain.Entities.Notes.NoteSection section)
        {
            return new NoteSectionResponse
            {
                Id = section.Id,
                Title = section.Title,
                SortOrder = section.SortOrder,

                Contents = section.Contents
                    .OrderBy(x => x.SortOrder)
                    .Select(x => new NoteContentResponse
                    {
                        Id = x.Id,
                        Type = x.Type,
                        SortOrder = x.SortOrder,
                        Content = x.Content,
                        Data = x.Data,
                        Metadata = x.Metadata
                    })
                    .ToList(),

                Children = section.Children
                    .OrderBy(x => x.SortOrder)
                    .Select(MapSection)
                    .ToList()
            };
        }
    }
}
