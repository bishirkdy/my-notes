using System.ComponentModel.DataAnnotations;
using MediatR;
using TechNotes.Application.Common.Exceptions;
using TechNotes.Application.DTOs.Categories;
using TechNotes.Application.Interfaces.Repositories;
using TechNotes.Domain.Entities.Notes;

namespace TechNotes.Application.Features.Categories.Commands.UpdateCategory;

public sealed class UpdateCategoryCommandHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<UpdateCategoryCommand, CategoryResponse?>
{
    public async Task<CategoryResponse?> Handle(
        UpdateCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetForUserAsync(
            command.CategoryId,
            command.UserId,
            cancellationToken);

        if (category is null)
        {
            return null;
        }

        var name = command.Request.Name.Trim();
        if (await categoryRepository.NameExistsAsync(
                command.UserId,
                name,
                category.Id,
                cancellationToken))
        {
            throw new ConflictException("A category with this name already exists.");
        }

        Category? parent = null;
        if (command.Request.ParentCategoryId.HasValue)
        {
            if (command.Request.ParentCategoryId.Value == category.Id)
            {
                throw new ValidationException("A category cannot be its own parent.");
            }

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

            if (category.Children.Count > 0)
            {
                throw new ValidationException("Move or remove this category's children before making it a child category.");
            }
        }

        category.Name = name;
        category.Description = string.IsNullOrWhiteSpace(command.Request.Description)
            ? null
            : command.Request.Description.Trim();
        category.ParentCategoryId = parent?.Id;
        category.ParentCategory = parent;
        category.SortOrder = command.Request.SortOrder;

        categoryRepository.Update(category);
        await categoryRepository.SaveChangesAsync(cancellationToken);

        return new CategoryResponse
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            ParentCategoryId = category.ParentCategoryId,
            ParentCategoryName = parent?.Name,
            SortOrder = category.SortOrder,
            NoteCount = category.Notes.Count,
            ChildCount = category.Children.Count,
            CreatedAtUtc = category.CreatedAtUtc
        };
    }
}
