namespace TechNotes.Domain.Enums;

// Determines the actions a recipient can perform on a shared note.
public enum SharePermission
{
    // The recipient can read the note only.
    View = 1,
    // The recipient can read and add comments.
    Comment = 2,
    // The recipient can change the note.
    Edit = 3
}
