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
    public void Validate_AcceptsImageContentWithUrlAndPublicId()
    {
        var sections = new[]
        {
            new NoteSectionRequest
            {
                Title = "Screenshots",
                Contents =
                [
                    new NoteContentRequest
                    {
                        Type = NoteContentType.Image,
                        Data = """{"url":"https://res.cloudinary.com/demo/image/upload/note.png","publicId":"technotes/note","width":1200,"height":800}"""
                    }
                ]
            }
        };

        NoteContentValidator.Validate(sections);
    }

    [Fact]
    public void Validate_AcceptsRootRelativeImageUrl()
    {
        var sections = new[]
        {
            new NoteSectionRequest
            {
                Title = "Screenshots",
                Contents =
                [
                    new NoteContentRequest
                    {
                        Type = NoteContentType.Image,
                        Data = """{"url":"/illustrations/handdrawn/v1/note.svg","publicId":"illustrations/handdrawn/v1/note","width":1600,"height":900}"""
                    }
                ]
            }
        };

        NoteContentValidator.Validate(sections);
    }

    [Fact]
    public void Validate_RejectsImageWithoutUrl()
    {
        var sections = new[]
        {
            new NoteSectionRequest
            {
                Title = "Screenshots",
                Contents =
                [
                    new NoteContentRequest
                    {
                        Type = NoteContentType.Image,
                        Data = """{"publicId":"technotes/note"}"""
                    }
                ]
            }
        };

        var error = Assert.Throws<ArgumentException>(() => NoteContentValidator.Validate(sections));
        Assert.Contains("valid URL", error.Message);
    }

    [Fact]
    public void Validate_RejectsImageWithoutPublicId()
    {
        var sections = new[]
        {
            new NoteSectionRequest
            {
                Title = "Screenshots",
                Contents =
                [
                    new NoteContentRequest
                    {
                        Type = NoteContentType.Image,
                        Data = """{"url":"https://res.cloudinary.com/demo/image/upload/note.png"}"""
                    }
                ]
            }
        };

        var error = Assert.Throws<ArgumentException>(() => NoteContentValidator.Validate(sections));
        Assert.Contains("public ID", error.Message);
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
