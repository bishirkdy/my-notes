using System.ComponentModel.DataAnnotations;
using MediatR;
using TechNotes.Application.DTOs.Notes;
using TechNotes.Application.Features.Notes.Common;
using TechNotes.Application.Interfaces.Repositories;
using TechNotes.Domain.Entities.Notes;

namespace TechNotes.Application.Features.Notes.Commands.UpdateNote
{
    public sealed class UpdateNoteCommandHandler(
        INoteRepository noteRepository,
        ICategoryRepository categoryRepository,
        ITagRepository tagRepository)
        : IRequestHandler<UpdateNoteCommand, NoteResponse?>
    {
        public async Task<NoteResponse?> Handle(UpdateNoteCommand command, CancellationToken cancellationToken)
        {
            var note = await noteRepository.GetByIdAsync(command.NoteId, cancellationToken);
            if (note is null || (!command.IsAdmin && note.UserId != command.UserId))
            {
                return null;
            }

            var request = command.Request;
            NoteContentValidator.Validate(request.Sections);

            Category? category = null;
            if (request.CategoryId.HasValue)
            {
                category = await categoryRepository.GetForUserAsync(
                    request.CategoryId.Value,
                    command.UserId,
                    cancellationToken);

                if (category is null)
                {
                    throw new ValidationException("The selected category was not found.");
                }
            }

            var noteTags = await NoteTagResolver.ResolveAsync(
                request.Tags,
                tagRepository,
                cancellationToken);

            note.Title = request.Title.Trim();
            note.Visibility = request.Visibility ?? note.Visibility;
            note.Status = request.Status ?? note.Status;
            note.FolderId = request.FolderId;
            note.CategoryId = category?.Id;
            note.Category = category;

            // Remove the old hierarchy before adding the new one. Saving both in a
            // single graph causes EF Core to update rows that it is also deleting.
            await noteRepository.RemoveSectionsAsync(note, cancellationToken);
            await noteRepository.RemoveTagsAsync(note, cancellationToken);
            await noteRepository.SaveChangesAsync(cancellationToken);

            note.Sections.Clear();
            note.NoteTags.Clear();

            AddSections(note, request.Sections);
            foreach (var noteTag in noteTags)
            {
                noteTag.NoteId = note.Id;
            }

            await noteRepository.AddTagsAsync(noteTags, cancellationToken);
            await noteRepository.SaveChangesAsync(cancellationToken);

            var savedNote = await noteRepository.GetWithContentAsync(note.Id, cancellationToken);
            return savedNote is null ? null : NoteResponseMapper.Map(savedNote);
        }

        private static void AddSections(
            Note note,
            IEnumerable<NoteSectionRequest> requests,
            NoteSection? parent = null)
        {
            foreach (var request in requests)
            {
                var section = new NoteSection
                {
                    Id = Guid.Empty,
                    Note = note,
                    ParentSection = parent,
                    Title = request.Title.Trim(),
                    SortOrder = request.SortOrder
                };

                foreach (var content in request.Contents)
                {
                    section.Contents.Add(new NoteContent
                    {
                        Id = Guid.Empty,
                        Section = section,
                        Type = content.Type,
                        SortOrder = content.SortOrder,
                        Content = content.Content,
                        Data = string.IsNullOrWhiteSpace(content.Data) ? null : content.Data,
                        Metadata = string.IsNullOrWhiteSpace(content.Metadata) ? null : content.Metadata
                    });
                }

                AddSections(note, request.Children, parent: section);

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
    }
}
