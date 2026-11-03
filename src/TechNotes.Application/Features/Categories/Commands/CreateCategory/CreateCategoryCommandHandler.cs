using System.ComponentModel.DataAnnotations;
using MediatR;
using TechNotes.Application.DTOs.Categories;
using TechNotes.Application.Interfaces.Repositories;
using TechNotes.Domain.Entities.Notes;

namespace TechNotes.Application.Features.Categories.Commands.CreateCategory;

public sealed class CreateCategoryCommandHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<CreateCategoryCommand, CategoryResponse?>
{
    public async Task<CategoryResponse?> Handle(
        CreateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var name = command.Request.Name.Trim();

        if (await categoryRepository.NameExistsAsync(
                command.UserId,
                name,
                cancellationToken: cancellationToken))
        {
            return null;
        }

        Category? parent = null;
        if (command.Request.ParentCategoryId.HasValue)
        {
            parent = await categoryRepository.GetForUserAsync(
                command.Request.ParentCategoryId.Value,
                command.UserId,
                cancellationToken);

            if (parent is null)
            {
                throw new ValidationException("The selected parent category was not found.");
            }

            if (parent.ParentCategoryId.HasValue)
            {
                throw new ValidationException("Categories support one grouping level. Select a top-level group as the parent.");
            }
        }

        var category = new Category
        {
            UserId = command.UserId,
            Name = name,
            Description = string.IsNullOrWhiteSpace(command.Request.Description)
                ? null
                : command.Request.Description.Trim(),
            ParentCategoryId = parent?.Id,
            ParentCategory = parent,
            SortOrder = command.Request.SortOrder
        };

        await categoryRepository.AddAsync(category, cancellationToken);
        await categoryRepository.SaveChangesAsync(cancellationToken);

        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = parent?.Name,
            SortOrder = category.SortOrder,
            NoteCount = 0,
            ChildCount = 0,
            CreatedAtUtc = category.CreatedAtUtc
        };
    }
}
