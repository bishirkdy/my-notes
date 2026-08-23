namespace TechNotes.Domain.Enums;

// Controls who can discover a note before individual share permissions are checked.
public enum NoteVisibility
{
    // Only the note owner can access it.
    Private = 1,
    // Only people explicitly given access can open it.
    Shared = 2,
    // Any visitor can view it when a public endpoint is added.
    Public = 3
}
