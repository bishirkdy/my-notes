using System.ComponentModel.DataAnnotations;
using MediatR;
using TechNotes.Application.DTOs.Notes;
using TechNotes.Application.Features.Notes.Common;
using TechNotes.Application.Interfaces.Repositories;
using TechNotes.Domain.Entities.Notes;

namespace TechNotes.Application.Features.Notes.Commands.CreateNote
{
    public sealed class CreateNoteCommandHandler(
        INoteRepository noteRepository,
        ICategoryRepository categoryRepository,
        ITagRepository tagRepository)
        : IRequestHandler<CreateNoteCommand, NoteResponse>
    {
        public async Task<NoteResponse> Handle(CreateNoteCommand command, CancellationToken cancellationToken)
        {
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

            var note = new Note
            {
                UserId = command.UserId,
                Title = request.Title.Trim(),
                Visibility = request.Visibility,
                Status = request.Status,
                FolderId = request.FolderId,
                CategoryId = category?.Id,
                Category = category
            };

            var noteTags = await NoteTagResolver.ResolveAsync(
                request.Tags,
                tagRepository,
                cancellationToken);

            note.NoteTags = noteTags;

            AddSections(note, request.Sections);

            await noteRepository.AddAsync(note, cancellationToken);
            await noteRepository.SaveChangesAsync(cancellationToken);

            return NoteResponseMapper.Map(note);
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
                    Note = note,
                    ParentSection = parent,
                    Title = request.Title.Trim(),
                    SortOrder = request.SortOrder
                };

                foreach (var content in request.Contents)
                {
                    section.Contents.Add(new NoteContent
                    {
                        Section = section,
                        Type = content.Type,
                        SortOrder = content.SortOrder,
                        Content = content.Content,
                        Data = string.IsNullOrWhiteSpace(content.Data) ? null : content.Data,
                        Metadata = string.IsNullOrWhiteSpace(content.Metadata) ? null : content.Metadata
                    });
                }

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
    }
}
