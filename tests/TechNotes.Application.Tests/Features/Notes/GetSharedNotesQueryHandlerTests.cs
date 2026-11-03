using NSubstitute;
using TechNotes.Application.Features.Notes.Queries.GetSharedNotes;
using TechNotes.Application.Interfaces.Repositories;
using TechNotes.Domain.Entities.Notes;
using TechNotes.Domain.Enums;

namespace TechNotes.Application.Tests.Features.Notes;

public sealed class GetSharedNotesQueryHandlerTests
{
    [Fact]
    public async Task Handle_ReturnsNotesSharedWithUser()
    {
        var userId = Guid.NewGuid();
        var repository = Substitute.For<INoteRepository>();
        repository.GetSharedWithMeAsync(userId, Arg.Any<CancellationToken>())
            .Returns([
                new Note
                {
                    Title = "Shared note",
                    Visibility = NoteVisibility.Shared,
                    Status = NoteStatus.Published
                }
            ]);

        var handler = new GetSharedNotesQueryHandler(repository);
        var result = await handler.Handle(new GetSharedNotesQuery(userId), CancellationToken.None);

        Assert.Single(result);
        Assert.Equal("Shared note", result[0].Title);
    }
}
