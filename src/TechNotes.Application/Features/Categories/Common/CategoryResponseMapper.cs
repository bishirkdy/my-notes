using TechNotes.Application.DTOs.Categories;
using TechNotes.Domain.Entities.Notes;

namespace TechNotes.Application.Features.Categories.Common;

public static class CategoryResponseMapper
{
    public static CategoryResponse Map(Category category) =>
        new()
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = category.ParentCategory?.Name,
            SortOrder = category.SortOrder,
            NoteCount = category.Notes.Count,
            ChildCount = category.Children.Count,
            CreatedAtUtc = category.CreatedAtUtc
        };

    public static IReadOnlyList<CategoryTreeResponse> BuildTree(IReadOnlyList<Category> categories)
    {
        var byId = categories.ToDictionary(category => category.Id);
        var children = categories
            .Where(category => category.ParentCategoryId.HasValue)
            .GroupBy(category => category.ParentCategoryId!.Value)
            .ToDictionary(group => group.Key, group => group.ToList());

        CategoryTreeResponse Build(Category category)
        {
            var childResponses = (children.GetValueOrDefault(category.Id) ?? [])
                .OrderBy(child => child.SortOrder)
                .ThenBy(child => child.Name)
                .Select(Build)
                .ToList();

            return new CategoryTreeResponse
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                SortOrder = category.SortOrder,
                NoteCount = category.Notes.Count + childResponses.Sum(child => child.NoteCount),
                Children = childResponses
            };
        }

        return categories
            .Where(category => category.ParentCategoryId is null || !byId.ContainsKey(category.ParentCategoryId.Value))
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Name)
            .Select(Build)
            .ToList();
    }
}
