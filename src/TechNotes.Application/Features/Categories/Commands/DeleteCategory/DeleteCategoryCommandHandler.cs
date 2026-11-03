using MediatR;
using TechNotes.Application.Common.Exceptions;
using TechNotes.Application.Interfaces.Repositories;

namespace TechNotes.Application.Features.Categories.Commands.DeleteCategory;

public sealed class DeleteCategoryCommandHandler(ICategoryRepository categoryRepository)
    : IRequestHandler<DeleteCategoryCommand, bool>
{
    public async Task<bool> Handle(
        DeleteCategoryCommand command,
        CancellationToken cancellationToken)
    {
        var category = await categoryRepository.GetForUserAsync(
            command.CategoryId,
            command.UserId,
            cancellationToken);

        if (category is null)
        {
            return false;
        }

        if (category.Notes.Count > 0)
        {
            throw new ConflictException("Move the notes in this category before deleting it.");
        }

        if (category.Children.Count > 0)
        {
            throw new ConflictException("Delete or move this group's child categories before deleting it.");
        }

        categoryRepository.Delete(category);
        await categoryRepository.SaveChangesAsync(cancellationToken);
        return true;
    }
}
