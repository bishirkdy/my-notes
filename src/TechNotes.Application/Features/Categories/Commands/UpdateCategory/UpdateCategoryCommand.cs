using MediatR;
using TechNotes.Application.DTOs.Categories;

namespace TechNotes.Application.Features.Categories.Commands.UpdateCategory;

public sealed record UpdateCategoryCommand(
    Guid CategoryId,
    Guid UserId,
    UpdateCategoryRequest Request) : IRequest<CategoryResponse?>;
