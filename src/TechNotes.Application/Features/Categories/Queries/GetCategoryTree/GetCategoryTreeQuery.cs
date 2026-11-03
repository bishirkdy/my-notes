using MediatR;
using TechNotes.Application.DTOs.Categories;

namespace TechNotes.Application.Features.Categories.Queries.GetCategoryTree;

public sealed record GetCategoryTreeQuery(Guid UserId) : IRequest<IReadOnlyList<CategoryTreeResponse>>;
