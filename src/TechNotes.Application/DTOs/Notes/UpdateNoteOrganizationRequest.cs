namespace TechNotes.Application.DTOs.Notes;

public sealed class UpdateNoteOrganizationRequest
{
    public Guid? CategoryId { get; set; }

    public Guid? FolderId { get; set; }

    public List<string> Tags { get; set; } = [];
}
