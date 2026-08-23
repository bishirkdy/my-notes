namespace TechNotes.Domain.Enums;

// Identifies why a notification was created.
public enum NotificationType
{
    // A note was shared with the user.
    NoteShared = 1,
    // Someone commented on a note.
    CommentAdded = 2,
    // The user was mentioned in a note or comment.
    Mentioned = 3,
    // A general system message from the application.
    System = 4
}
