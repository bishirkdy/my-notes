using NSubstitute;
using TechNotes.Application.DTOs.Notes;
using TechNotes.Application.Features.Notes.Commands.UpdateNote;
using TechNotes.Application.Interfaces.Repositories;
using TechNotes.Domain.Entities.Notes;
using TechNotes.Domain.Enums;

namespace TechNotes.Application.Tests.Features.Notes;

public sealed class UpdateNoteCommandHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsNull_WhenNoteBelongsToAnotherUser()
    {
        var repository = Substitute.For<INoteRepository>();
        var note = new Note { Id = Guid.NewGuid(), UserId = Guid.NewGuid(), Title = "Existing" };
        repository.GetByIdAsync(note.Id, Arg.Any<CancellationToken>()).Returns(note);

        var handler = new UpdateNoteCommandHandler(repository);
        var result = await handler.Handle(
            new UpdateNoteCommand(note.Id, Guid.NewGuid(), new UpdateNoteRequest { Title = "Updated" }),
            CancellationToken.None);

        Assert.Null(result);
        await repository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_ReplacesSectionsAndUpdatesPublishingState()
    {
        var repository = Substitute.For<INoteRepository>();
        var note = new Note
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Title = "Old title",
            Visibility = NoteVisibility.Private,
            Status = NoteStatus.Draft
        };

        var savedNote = new Note
        {
            Id = note.Id,
            UserId = note.UserId,
            Title = "New title",
            Visibility = NoteVisibility.Public,
            Status = NoteStatus.Published,
            Sections =
            [
                new NoteSection
                {
                    Title = "Overview",
                    SortOrder = 1,
                    Contents =
                    [
                        new NoteContent
                        {
                            Type = NoteContentType.Quote,
                            SortOrder = 1,
                            Data = """{"quote":"Make it work."}"""
                        }
                    ]
                }
            ]
        };

        repository.GetByIdAsync(note.Id, Arg.Any<CancellationToken>()).Returns(note);
        repository.GetWithContentAsync(note.Id, Arg.Any<CancellationToken>()).Returns(savedNote);
        repository.RemoveSectionsAsync(note, Arg.Any<CancellationToken>()).Returns(Task.CompletedTask);

        var request = new UpdateNoteRequest
        {
            Title = "New title",
            Visibility = NoteVisibility.Public,
            Status = NoteStatus.Published,
            Sections =
            [
                new NoteSectionRequest
                {
                    Title = "Overview",
                    SortOrder = 1,
                    Contents =
                    [
                        new NoteContentRequest
                        {
                            Type = NoteContentType.Quote,
                            SortOrder = 1,
                            Data = """{"quote":"Make it work."}"""
                        }
                    ]
                }
            ]
        };

        var handler = new UpdateNoteCommandHandler(repository);
        var result = await handler.Handle(
            new UpdateNoteCommand(note.Id, note.UserId, request),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal("New title", note.Title);
        Assert.Equal(NoteVisibility.Public, note.Visibility);
        Assert.Equal(NoteStatus.Published, note.Status);
        Assert.Single(result.Sections);
        Assert.Equal("Overview", result.Sections.Single().Title);
        Assert.Equal(NoteContentType.Quote, result.Sections.Single().Contents.Single().Type);
        await repository.Received(1).RemoveSectionsAsync(note, Arg.Any<CancellationToken>());
        await repository.Received(2).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_KeepsExistingState_WhenOptionalFieldsAreMissing()
    {
        var repository = Substitute.For<INoteRepository>();
        var note = new Note
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Title = "Title",
            Visibility = NoteVisibility.Shared,
            Status = NoteStatus.Published,
            Sections = [new NoteSection { Title = "Section" }]
        };

        repository.GetByIdAsync(note.Id, Arg.Any<CancellationToken>()).Returns(note);

        var request = new UpdateNoteRequest
        {
            Title = "Title",
            Sections = [new NoteSectionRequest { Title = "Section" }]
        };

        var handler = new UpdateNoteCommandHandler(repository);
        await handler.Handle(new UpdateNoteCommand(note.Id, note.UserId, request), CancellationToken.None);

        Assert.Equal(NoteVisibility.Shared, note.Visibility);
        Assert.Equal(NoteStatus.Published, note.Status);
    }
}
