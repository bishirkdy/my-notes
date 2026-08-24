namespace TechNotes.Domain.Enums;
// Describes the lifecycle state of a note.
public enum NoteStatus
{
    // The note is still being written.
    Draft = 1,
    // The note is ready to be shown or shared.
    Published = 2,
    // The note is kept but removed from normal active lists.
    Archived = 3
}
