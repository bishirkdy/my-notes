using MediatR;
using TechNotes.Application.DTOs.Notes;
using TechNotes.Application.Interfaces.Repositories;
using TechNotes.Domain.Entities.Notes;
using TechNotes.Domain.Enums;

namespace TechNotes.Application.Features.Notes.Commands.CreateNote
{
    // Handles the business operation of creating a new note.
    public sealed class CreateNoteCommandHandler(INoteRepository noteRepository) : IRequestHandler<CreateNoteCommand, NoteResponse>
    {
        // Creates the note and its complete section/content hierarchy.
        public async Task<NoteResponse> Handle(CreateNoteCommand command, CancellationToken cancellationToken)
        {
            // The authenticated user's ID becomes the owner of the note.
            var note = new Note
            {
                UserId = command.UserId,
                Title = command.Request.Title,
                Visibility = command.Request.Visibility,
                FolderId = command.Request.FolderId,
                CategoryId = command.Request.CategoryId,
                Status = NoteStatus.Draft
            };

            // Converts the request's nested sections into domain entities.
            AddSections(note, command.Request.Sections);

            // Saves the complete note graph through the repository.
            await noteRepository.AddAsync(note, cancellationToken);

            // TODO: Replace this with your existing UnitOfWork/SaveChanges mechanism.
            await noteRepository.SaveChangesAsync(cancellationToken);

            // Converts the entity into the API response DTO.
            return MapToResponse(note);
        }

        // Recursively creates headings and subheadings.
        private static void AddSections(Note note, IEnumerable<NoteSectionRequest> requests, NoteSection? parent = null)
        {
            foreach (var request in requests)
            {
                var section = new NoteSection
                {
                    Note = note,
                    ParentSection = parent,
                    Title = request.Title,
                    SortOrder = request.SortOrder
                };

                // Adds all content blocks belonging to this section.
                foreach (var content in request.Contents)
                {
                    section.Contents.Add(new NoteContent
                    {
                        Section = section,
                        Type = content.Type,
                        SortOrder = content.SortOrder,
                        Content = content.Content,
                        Data = content.Data,
                        Metadata = content.Metadata
                    });
                }

                // Recursively creates nested subheadings.
                AddSections(note, request.Children, section);

                if (parent is null)
                {
                    note.Sections.Add(section);
                }
                else
                {
                    parent.Children.Add(section);
                }
            }
        }

        // Maps the domain entity to the response DTO.
        private static NoteResponse MapToResponse(Note note)
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

                // Returns only root sections; child sections are mapped recursively.
                Sections = note.Sections
                    .Where(x => x.ParentSectionId == null)
                    .OrderBy(x => x.SortOrder)
                    .Select(MapSection)
                    .ToList()
            };
        }

        // Recursively maps a section and its children/content.
        private static NoteSectionResponse MapSection(NoteSection section)
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
