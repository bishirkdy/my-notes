using System.ComponentModel.DataAnnotations;

namespace TechNotes.Application.DTOs.Categories;

public sealed class UpdateCategoryRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Description { get; set; }

    public Guid? ParentCategoryId { get; set; }

    public int SortOrder { get; set; }
}
