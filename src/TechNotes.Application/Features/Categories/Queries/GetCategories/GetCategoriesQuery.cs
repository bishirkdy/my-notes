using MediatR;
using TechNotes.Application.DTOs.Categories;

namespace TechNotes.Application.Features.Categories.Queries.GetCategories;

public sealed record GetCategoriesQuery(Guid UserId)
    : IRequest<IReadOnlyList<CategoryResponse>>;
