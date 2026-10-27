using TechNotes.Application.DTOs.Notes;
using TechNotes.Application.Features.Notes.Common;
using TechNotes.Domain.Enums;

namespace TechNotes.Application.Tests.Features.Notes;

public sealed class NoteContentValidatorTests
{
    [Fact]
    public void Validate_AcceptsNestedSectionsWithValidContent()
    {
        var sections = new List<NoteSectionRequest>
        {
            new()
            {
                Title = "Root",
                Contents =
                [
                    new NoteContentRequest
                    {
                        Type = NoteContentType.Code,
                        Content = "var note = new Note();",
                        Data = """{"code":"var note = new Note();"}""",
                        Metadata = """{"language":"csharp"}"""
                    }
                ],
                Children =
                [
                    new NoteSectionRequest
                    {
                        Title = "Child",
                        Contents = [new NoteContentRequest { Type = NoteContentType.Text, Content = "Hello" }]
                    }
                ]
            }
        };

        NoteContentValidator.Validate(sections);
    }

    [Fact]
    public void Validate_RejectsEmptySectionTitle()
    {
        var sections = new[] { new NoteSectionRequest { Title = " " } };

        var error = Assert.Throws<ArgumentException>(() => NoteContentValidator.Validate(sections));

        Assert.Contains("section must have a title", error.Message);
    }

    [Fact]
    public void Validate_RejectsInvalidJson()
    {
        var sections = new[]
        {
            new NoteSectionRequest
            {
                Title = "Root",
                Contents = [new NoteContentRequest { Type = NoteContentType.Table, Data = "{not-json" }]
            }
        };

        var error = Assert.Throws<ArgumentException>(() => NoteContentValidator.Validate(sections));

        Assert.Contains("valid JSON", error.Message);
    }

    [Fact]
    public void Validate_RejectsUnknownContentType()
    {
        var sections = new[]
        {
            new NoteSectionRequest
            {
                Title = "Root",
                Contents = [new NoteContentRequest { Type = (NoteContentType)99 }]
            }
        };

        var error = Assert.Throws<ArgumentException>(() => NoteContentValidator.Validate(sections));

        Assert.Contains("not supported", error.Message);
    }

    [Fact]
    public void Validate_RejectsInvalidNestedSection()
    {
        var sections = new[]
        {
            new NoteSectionRequest
            {
                Title = "Root",
                Children = [new NoteSectionRequest { Title = "" }]
            }
        };

        Assert.Throws<ArgumentException>(() => NoteContentValidator.Validate(sections));
    }
}
