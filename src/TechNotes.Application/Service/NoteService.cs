using TechNotes.Application.DTOs.Notes;
using TechNotes.Application.Interfaces.Repositories;
using TechNotes.Application.Interfaces.Services.Business.Notes;
using TechNotes.Domain.Entities.Notes;
using TechNotes.Domain.Enums;

namespace TechNotes.Application.Service;

// Contains the business rules for creating and managing notes.
public sealed class NoteService(
    INoteRepository noteRepository) : INoteService
{
    // Creates a new note owned by the authenticated user.
    public async Task<NoteResponse> CreateAsync(Guid userId,CreateNoteRequest request,CancellationToken cancellationToken = default)
    {
        // The authenticated user's ID is used as the note owner.
        var note = new Note
        {
            UserId = userId,
            Title = request.Title,
            Visibility = request.Visibility,
            FolderId = request.FolderId,
            CategoryId = request.CategoryId,
            Status = NoteStatus.Draft
        };

        // Converts the DTO section tree into domain entities.
        AddSections(note, request.Sections);

        await noteRepository.AddAsync(note, cancellationToken);

        return MapToResponse(note);
    }

    // Retrieves a note when the current user has permission to view it.
    public async Task<NoteResponse?> GetByIdAsync(
        Guid noteId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetWithContentAsync(
            noteId,
            cancellationToken);

        if (note is null)
        {
            return null;
        }

        // The owner can always view their own note.
        if (note.UserId == userId)
        {
            return MapToResponse(note);
        }

        // Public notes can be viewed by everyone.
        if (note.Visibility == NoteVisibility.Public)
        {
            return MapToResponse(note);
        }

        // Shared notes require an explicit share record.
        if (note.Visibility == NoteVisibility.Shared &&
            note.Shares.Any(x => x.UserId == userId))
        {
            return MapToResponse(note);
        }

        // The user does not have permission to view this note.
        return null;
    }

    // Retrieves all notes owned by the authenticated user.
    public async Task<IReadOnlyList<NoteResponse>> GetMyNotesAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var notes = await noteRepository.GetByUserIdAsync(
            userId,
            cancellationToken);

        return notes
            .Select(MapToResponse)
            .ToList();
    }

    // Updates a note when the user is the owner or has Edit permission.
    public async Task<NoteResponse?> UpdateAsync(
        Guid noteId,
        Guid userId,
        UpdateNoteRequest request,
        CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetWithContentAsync(
            noteId,
            cancellationToken);

        if (note is null)
        {
            return null;
        }

        // The owner always has permission to edit.
        var isOwner = note.UserId == userId;

        // A shared user needs Edit permission.
        var isEditor = note.Shares.Any(x =>
            x.UserId == userId &&
            x.Permission == SharePermission.Edit);

        if (!isOwner && !isEditor)
        {
            return null;
        }

        note.Title = request.Title;
        note.Visibility = request.Visibility;
        note.FolderId = request.FolderId;
        note.CategoryId = request.CategoryId;

        // Remove the old section tree before adding the updated structure.
        note.Sections.Clear();

        // Rebuild the section/content hierarchy from the request.
        AddSections(note, request.Sections);

        noteRepository.Update(note);

        return MapToResponse(note);
    }

    // Deletes a note only when the authenticated user owns it.
    public async Task<bool> DeleteAsync(
        Guid noteId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var note = await noteRepository.GetByIdAsync(
            noteId,
            cancellationToken);

        if (note is null || note.UserId != userId)
        {
            return false;
        }

        noteRepository.Delete(note);

        return true;
    }

    // Recursively creates sections and nested child sections.
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
                Title = request.Title,
                SortOrder = request.SortOrder
            };

            // Add definition, list, table, example, code, graph, etc.
            AddContents(section, request.Contents);

            // Recursively process nested subheadings.
            AddSections(note, request.Children, section);

            // Attach the section to either the note or its parent.
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

    // Creates content blocks belonging to a section.
    private static void AddContents(
        NoteSection section,
        IEnumerable<NoteContentRequest> requests)
    {
        foreach (var request in requests)
        {
            section.Contents.Add(new NoteContent
            {
                Section = section,
                Type = request.Type,
                SortOrder = request.SortOrder,
                Content = request.Content,
                Data = request.Data,
                Metadata = request.Metadata
            });
        }
    }

    // Converts a Note entity into a response DTO.
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

            // Only root sections are selected here.
            Sections = note.Sections
                .Where(x => x.ParentSectionId == null)
                .OrderBy(x => x.SortOrder)
                .Select(MapSection)
                .ToList()
        };
    }

    // Recursively converts sections and their children to response DTOs.
    private static NoteSectionResponse MapSection(NoteSection section)
    {
        return new NoteSectionResponse
        {
            Id = section.Id,
            Title = section.Title,
            SortOrder = section.SortOrder,

            // Maps all content blocks inside this section.
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

            // Recursively maps nested headings.
            Children = section.Children
                .OrderBy(x => x.SortOrder)
                .Select(MapSection)
                .ToList()
        };
    }
}