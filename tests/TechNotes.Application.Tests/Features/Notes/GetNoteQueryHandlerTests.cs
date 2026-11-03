using NSubstitute;
using TechNotes.Application.Features.Notes.Queries.GetNote;
using TechNotes.Application.Interfaces.Repositories;
using TechNotes.Domain.Entities.Notes;
using TechNotes.Domain.Enums;

namespace TechNotes.Application.Tests.Features.Notes;

public sealed class GetNoteQueryHandlerTests
{
    private readonly INoteRepository _repository = Substitute.For<INoteRepository>();
    private readonly IReadingProgressRepository _progressRepository = Substitute.For<IReadingProgressRepository>();
    private readonly GetNoteQueryHandler _handler;

    public GetNoteQueryHandlerTests()
    {
        _handler = new GetNoteQueryHandler(_repository, _progressRepository);
    }

    [Fact]
    public async Task Handle_ReturnsPublishedPublicNoteToAnonymousUser()
    {
        var note = CreateNote(NoteVisibility.Public, NoteStatus.Published);
        _repository.GetWithContentAsync(note.Id, Arg.Any<CancellationToken>()).Returns(note);

        var result = await _handler.Handle(new GetNoteQuery(note.Id, null), CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(note.Title, result.Title);
    }

    [Fact]
    public async Task Handle_HidesDraftPublicNoteFromAnonymousUser()
    {
        var note = CreateNote(NoteVisibility.Public, NoteStatus.Draft);
        _repository.GetWithContentAsync(note.Id, Arg.Any<CancellationToken>()).Returns(note);

        var result = await _handler.Handle(new GetNoteQuery(note.Id, null), CancellationToken.None);

        Assert.Null(result);
    }

    [Fact]
    public async Task Handle_AllowsOwnerToReadPrivateDraft()
    {
        var note = CreateNote(NoteVisibility.Private, NoteStatus.Draft);
        _repository.GetWithContentAsync(note.Id, Arg.Any<CancellationToken>()).Returns(note);

        var result = await _handler.Handle(new GetNoteQuery(note.Id, note.UserId), CancellationToken.None);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task Handle_AllowsUserWithShareToReadSharedNote()
    {
        var note = CreateNote(NoteVisibility.Shared, NoteStatus.Draft);
        var viewerId = Guid.NewGuid();
        note.Shares.Add(new NoteShare { NoteId = note.Id, UserId = viewerId });
        _repository.GetWithContentAsync(note.Id, Arg.Any<CancellationToken>()).Returns(note);

        var result = await _handler.Handle(new GetNoteQuery(note.Id, viewerId), CancellationToken.None);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task Handle_HidesSharedNoteFromUserWithoutShare()
    {
        var note = CreateNote(NoteVisibility.Shared, NoteStatus.Published);
        _repository.GetWithContentAsync(note.Id, Arg.Any<CancellationToken>()).Returns(note);

        var result = await _handler.Handle(new GetNoteQuery(note.Id, Guid.NewGuid()), CancellationToken.None);

        Assert.Null(result);
    }

    private static Note CreateNote(NoteVisibility visibility, NoteStatus status) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Title = "Note",
            Visibility = visibility,
            Status = status,
            Sections =
            [
                new NoteSection
                {
                    Title = "Overview",
                    SortOrder = 1,
                    Contents = [new NoteContent { Type = NoteContentType.Text, SortOrder = 1, Content = "Body" }]
                }
            ]
        };
}
