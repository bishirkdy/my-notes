using MediatR;
using TechNotes.Application.DTOs.Notes;
using TechNotes.Application.Interfaces.Repositories;
using TechNotes.Domain.Enums;

namespace TechNotes.Application.Features.Notes.Queries.GetNote
{
    // Handles retrieving a note while applying its visibility rules.
    public class GetNoteQueryHandler(INoteRepository noteRepository) : IRequestHandler<GetNoteQuery , NoteResponse?>
    {
        public async Task<NoteResponse> Handle(GetNoteQuery query , CancellationToken cancellationToken)
        {
            // Retrieves the note with its related content.
            var note = await noteRepository.GetWithContentAsync(query.NoteId, cancellationToken);

            // Return null when the note does not exist.
            if(note is null)
            {
                return null;
            }

            // The owner can always access their own note.
            if (query.UserId.HasValue &&
                note.UserId == query.UserId.Value)
            {
                return MapToResponse(note);
            }

            // Public notes can be accessed by anyone, including anonymous visitors.
            if (note.Visibility == NoteVisibility.Public)
            {
                return MapToResponse(note);
            }

            // Shared notes require an authenticated user with a share record.
            if (query.UserId.HasValue &&
                note.Visibility == NoteVisibility.Shared &&
                note.Shares.Any(x => x.UserId == query.UserId.Value))
            {
                return MapToResponse(note);
            }

            // The user does not have permission to view the note.
            return null;
        }

        // Converts the Note entity into an API response DTO.
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

                // Maps root sections and recursively maps their children.
                Sections = note.Sections
                    .Where(x => x.ParentSectionId == null)
                    .OrderBy(x => x.SortOrder)
                    .Select(MapSection)
                    .ToList()
            };
        }

        // Recursively maps a section and all nested sections.
        private static NoteSectionResponse MapSection(
            Domain.Entities.Notes.NoteSection section)
        {
            return new NoteSectionResponse
            {
                Id = section.Id,
                Title = section.Title,
                SortOrder = section.SortOrder,

                // Maps definition, points, examples, tables, code, graphs, etc.
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

                // Recursively maps child headings.
                Children = section.Children
                    .OrderBy(x => x.SortOrder)
                    .Select(MapSection)
                    .ToList()
            };
        }
    }
}
