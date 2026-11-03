namespace TechNotes.Application.DTOs.Categories;

public sealed class CategoryResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public Guid? ParentCategoryId { get; set; }

    public string? ParentCategoryName { get; set; }

    public int SortOrder { get; set; }

    public int NoteCount { get; set; }

    public int ChildCount { get; set; }

    public DateTime CreatedAtUtc { get; set; }
}
