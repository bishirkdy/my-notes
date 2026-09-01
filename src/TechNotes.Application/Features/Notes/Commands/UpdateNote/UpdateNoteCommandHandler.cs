using MediatR;
using TechNotes.Application.DTOs.Notes;
using TechNotes.Application.Interfaces.Repositories;

namespace TechNotes.Application.Features.Notes.Commands.UpdateNote
{
    // Handles updating a note owned by the authenticated user.
    public sealed class UpdateNoteCommandHandler(INoteRepository noteRepository) : IRequestHandler<UpdateNoteCommand, NoteResponse?>
    {
        // Updates the note after verifying ownership.
        public async Task<NoteResponse?> Handle(UpdateNoteCommand command, CancellationToken cancellationToken)
        {
            var note = await noteRepository.GetWithContentAsync(command.NoteId, cancellationToken);

            if (note is null || note.UserId != command.UserId)
            {
                return null;
            }

            note.Title = command.Request.Title;
            note.Visibility = command.Request.Visibility;
            note.FolderId = command.Request.FolderId;
            note.CategoryId = command.Request.CategoryId;

            noteRepository.Update(note);
            await noteRepository.SaveChangesAsync(cancellationToken);
            return MapToResponse(note);
        }

        // Maps the updated note to the response DTO.
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