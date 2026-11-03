using MediatR;
using TechNotes.Application.DTOs.Categories;
using TechNotes.Application.Features.Categories.Common;
using TechNotes.Application.Interfaces.Repositories;

namespace TechNotes.Application.Features.Categories.Queries.GetCategories;

public sealed class GetCategoriesQueryHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<GetCategoriesQuery, IReadOnlyList<CategoryResponse>>
{
    public async Task<IReadOnlyList<CategoryResponse>> Handle(
        GetCategoriesQuery query,
        CancellationToken cancellationToken)
    {
        var categories = await categoryRepository.GetByUserIdAsync(
            query.UserId,
            cancellationToken);

        return categories
            .OrderBy(category => category.ParentCategoryId is null ? 0 : 1)
            .ThenBy(category => category.ParentCategory?.SortOrder ?? category.SortOrder)
            .ThenBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .Select(CategoryResponseMapper.Map)
            .ToList();
    }
}
