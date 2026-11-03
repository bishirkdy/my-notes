using MediatR;
using TechNotes.Application.DTOs.Categories;

namespace TechNotes.Application.Features.Categories.Commands.CreateCategory;

public sealed record CreateCategoryCommand(
    Guid UserId,
    CreateCategoryRequest Request) : IRequest<CategoryResponse?>;
