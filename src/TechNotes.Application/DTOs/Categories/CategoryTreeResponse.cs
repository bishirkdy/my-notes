namespace TechNotes.Application.DTOs.Categories;

public sealed class CategoryTreeResponse
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public int NoteCount { get; set; }

    public List<CategoryTreeResponse> Children { get; set; } = [];
}
