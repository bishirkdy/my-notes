using NSubstitute;
using TechNotes.Application.DTOs.Notes;
using TechNotes.Application.Features.Notes.Commands.CreateNote;
using TechNotes.Application.Interfaces.Repositories;
using TechNotes.Domain.Entities.Notes;
using TechNotes.Domain.Enums;

namespace TechNotes.Application.Tests.Features.Notes;

public sealed class CreateNoteCommandHandlerTests
{
    [Fact]
    public async Task Handle_PersistsPublishingStateAndNestedSections()
    {
        var repository = Substitute.For<INoteRepository>();
        Note? created = null;

        repository
            .AddAsync(Arg.Do<Note>(note => created = note), Arg.Any<CancellationToken>())
            .Returns(Task.CompletedTask);

        var handler = new CreateNoteCommandHandler(
            repository,
            Substitute.For<ICategoryRepository>(),
            Substitute.For<ITagRepository>());
        var userId = Guid.NewGuid();
        var request = new CreateNoteRequest
        {
            Title = "  Dependency injection  ",
            Visibility = NoteVisibility.Private,
            Status = NoteStatus.Draft,
            Sections =
            [
                new NoteSectionRequest
                {
                    Title = "Lifetime",
                    SortOrder = 1,
                    Contents =
                    [
                        new NoteContentRequest
                        {
                            Type = NoteContentType.Text,
                            SortOrder = 1,
                            Content = "Scoped services are created once per request."
                        }
                    ],
                    Children =
                    [
                        new NoteSectionRequest
                        {
                            Title = "Singleton",
                            SortOrder = 1,
                            Contents =
                            [
                                new NoteContentRequest
                                {
                                    Type = NoteContentType.CorePoints,
                                    SortOrder = 1,
                                    Data = """{"points":["One instance per application"]}"""
                                }
                            ]
                        }
                    ]
                }
            ]
        };

        var response = await handler.Handle(new CreateNoteCommand(userId, request), CancellationToken.None);

        Assert.NotNull(created);
        Assert.Equal(userId, created.UserId);
        Assert.Equal("Dependency injection", created.Title);
        Assert.Equal(NoteVisibility.Private, created.Visibility);
        Assert.Equal(NoteStatus.Draft, created.Status);
        Assert.Single(created.Sections);
        Assert.Single(created.Sections.Single().Children);
        Assert.Equal("Scoped services are created once per request.", created.Sections.Single().Contents.Single().Content);

        Assert.Equal("Dependency injection", response.Title);
        Assert.Equal(NoteStatus.Draft, response.Status);
        Assert.Single(response.Sections.Single().Children);
    }
}
