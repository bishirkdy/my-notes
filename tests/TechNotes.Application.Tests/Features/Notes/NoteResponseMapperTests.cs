using TechNotes.Application.Features.Notes.Common;
using TechNotes.Domain.Entities.Notes;
using TechNotes.Domain.Enums;

namespace TechNotes.Application.Tests.Features.Notes;

public sealed class NoteResponseMapperTests
{
    [Fact]
    public void Map_IncludesCategoryTagsAndTimestamps()
    {
        var createdAt = new DateTime(2026, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var updatedAt = createdAt.AddHours(2);
        var note = new Note
        {
            Title = "EF Core",
            Status = NoteStatus.Published,
            Visibility = NoteVisibility.Public,
            Category = new Category { Name = "Backend" },
            CreatedAtUtc = createdAt,
            UpdatedAtUtc = updatedAt,
            NoteTags =
            [
                new NoteTag { Tag = new Tag { Name = "dotnet" } },
                new NoteTag { Tag = new Tag { Name = "ef-core" } }
            ]
        };

        var response = NoteResponseMapper.Map(note);

        Assert.Equal("Backend", response.CategoryName);
        Assert.Equal(["dotnet", "ef-core"], response.Tags);
        Assert.Equal(createdAt, response.CreatedAtUtc);
        Assert.Equal(updatedAt, response.UpdatedAtUtc);
    }
}
