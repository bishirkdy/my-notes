using NSubstitute;
using TechNotes.Application.DTOs.Notes;
using TechNotes.Application.Features.Notes.Commands.SaveReadingProgress;
using TechNotes.Application.Interfaces.Repositories;
using TechNotes.Domain.Entities.Notes;
using TechNotes.Domain.Enums;

namespace TechNotes.Application.Tests.Features.Notes;

public sealed class SaveReadingProgressCommandHandlerTests
{
    private readonly INoteRepository _noteRepository = Substitute.For<INoteRepository>();
    private readonly IReadingProgressRepository _progressRepository = Substitute.For<IReadingProgressRepository>();

    [Fact]
    public async Task Handle_ClampsNewProgressAndReturnsSavedValue()
    {
        var note = CreateNote(NoteVisibility.Public, NoteStatus.Published);
        _noteRepository.GetByIdAsync(note.Id, Arg.Any<CancellationToken>()).Returns(note);
        ReadingProgress? added = null;
        await _progressRepository.AddAsync(
            Arg.Do<ReadingProgress>(item => added = item),
            Arg.Any<CancellationToken>());

        var handler = new SaveReadingProgressCommandHandler(_noteRepository, _progressRepository);
        var result = await handler.Handle(
            new SaveReadingProgressCommand(note.Id, Guid.NewGuid(), new SaveReadingProgressRequest { Percent = 140 }),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(100, result.Percent);
        Assert.NotNull(added);
        Assert.Equal(100, added.Percent);
        await _progressRepository.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_KeepsHighestProgress()
    {
        var note = CreateNote(NoteVisibility.Public, NoteStatus.Published);
        var userId = Guid.NewGuid();
        var progress = new ReadingProgress
        {
            NoteId = note.Id,
            UserId = userId,
            Percent = 70,
            LastReadAtUtc = DateTime.UtcNow.AddDays(-1)
        };

        _noteRepository.GetByIdAsync(note.Id, Arg.Any<CancellationToken>()).Returns(note);
        _progressRepository.GetAsync(userId, note.Id, Arg.Any<CancellationToken>()).Returns(progress);

        var handler = new SaveReadingProgressCommandHandler(_noteRepository, _progressRepository);
        var result = await handler.Handle(
            new SaveReadingProgressCommand(note.Id, userId, new SaveReadingProgressRequest { Percent = 25 }),
            CancellationToken.None);

        Assert.NotNull(result);
        Assert.Equal(70, progress.Percent);
        Assert.True(progress.LastReadAtUtc > DateTime.UtcNow.AddMinutes(-1));
        _progressRepository.Received(1).Update(progress);
    }

    [Fact]
    public async Task Handle_ReturnsNullForInaccessibleNote()
    {
        var note = CreateNote(NoteVisibility.Private, NoteStatus.Draft);
        _noteRepository.GetByIdAsync(note.Id, Arg.Any<CancellationToken>()).Returns(note);
        var handler = new SaveReadingProgressCommandHandler(_noteRepository, _progressRepository);

        var result = await handler.Handle(
            new SaveReadingProgressCommand(note.Id, Guid.NewGuid(), new SaveReadingProgressRequest { Percent = 40 }),
            CancellationToken.None);

        Assert.Null(result);
        await _progressRepository.DidNotReceive().SaveChangesAsync(Arg.Any<CancellationToken>());
    }

    private static Note CreateNote(NoteVisibility visibility, NoteStatus status) =>
        new()
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Title = "Note",
            Visibility = visibility,
            Status = status
        };
}
