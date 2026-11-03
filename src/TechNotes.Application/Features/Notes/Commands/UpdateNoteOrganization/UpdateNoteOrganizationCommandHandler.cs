using System.ComponentModel.DataAnnotations;
using MediatR;
using TechNotes.Application.DTOs.Notes;
using TechNotes.Application.Features.Notes.Common;
using TechNotes.Application.Interfaces.Repositories;
using TechNotes.Domain.Entities.Notes;

namespace TechNotes.Application.Features.Notes.Commands.UpdateNoteOrganization;

public sealed class UpdateNoteOrganizationCommandHandler(
    INoteRepository noteRepository,
    ICategoryRepository categoryRepository,
    ITagRepository tagRepository)
    : IRequestHandler<UpdateNoteOrganizationCommand, NoteResponse?>
{
    public async Task<NoteResponse?> Handle(
        UpdateNoteOrganizationCommand command,
        CancellationToken cancellationToken)
    {
        var note = await noteRepository.GetByIdAsync(command.NoteId, cancellationToken);
        if (note is null || (!command.IsAdmin && note.UserId != command.UserId))
        {
            return null;
        }

        Category? category = null;
        if (command.Request.CategoryId.HasValue)
        {
            category = await categoryRepository.GetForUserAsync(
                command.Request.CategoryId.Value,
                command.UserId,
                cancellationToken);

            if (category is null)
            {
                throw new ValidationException("The selected category was not found.");
            }
        }

        var noteTags = await NoteTagResolver.ResolveAsync(
            command.Request.Tags,
            tagRepository,
            cancellationToken);

        note.CategoryId = category?.Id;
        note.Category = category;
        note.FolderId = command.Request.FolderId;

        await noteRepository.RemoveTagsAsync(note, cancellationToken);
        await noteRepository.SaveChangesAsync(cancellationToken);

        note.NoteTags.Clear();
        foreach (var noteTag in noteTags)
        {
            noteTag.NoteId = note.Id;
        }

        await noteRepository.AddTagsAsync(noteTags, cancellationToken);
        await noteRepository.SaveChangesAsync(cancellationToken);

        var savedNote = await noteRepository.GetWithContentAsync(note.Id, cancellationToken);
        return savedNote is null ? null : NoteResponseMapper.Map(savedNote);
    }
}
