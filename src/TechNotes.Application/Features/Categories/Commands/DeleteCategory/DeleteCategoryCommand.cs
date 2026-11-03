using MediatR;

namespace TechNotes.Application.Features.Categories.Commands.DeleteCategory;

public sealed record DeleteCategoryCommand(Guid CategoryId, Guid UserId) : IRequest<bool>;
