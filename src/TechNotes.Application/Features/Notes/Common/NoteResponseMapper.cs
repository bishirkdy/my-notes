using TechNotes.Application.DTOs.Notes;
using TechNotes.Domain.Entities.Notes;

namespace TechNotes.Application.Features.Notes.Common;

public static class NoteResponseMapper
{
    public static NoteResponse Map(Note note) =>
        new()
        {
            Id = note.Id,
            UserId = note.UserId,
            Title = note.Title,
            Status = note.Status,
            Visibility = note.Visibility,
            FolderId = note.FolderId,
            CategoryId = note.CategoryId,
            Sections = note.Sections
                .Where(section => section.ParentSectionId is null)
                .OrderBy(section => section.SortOrder)
                .Select(MapSection)
                .ToList()
        };

    private static NoteSectionResponse MapSection(NoteSection section) =>
        new()
        {
            Id = section.Id,
            Title = section.Title,
            SortOrder = section.SortOrder,
            Contents = section.Contents
                .OrderBy(content => content.SortOrder)
                .Select(content => new NoteContentResponse
                {
                    Id = content.Id,
                    Type = content.Type,
                    SortOrder = content.SortOrder,
                    Content = content.Content,
                    Data = content.Data,
                    Metadata = content.Metadata
                })
                .ToList(),
            Children = section.Children
                .OrderBy(child => child.SortOrder)
                .Select(MapSection)
                .ToList()
        };
}
