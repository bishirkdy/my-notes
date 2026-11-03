namespace TechNotes.Application.DTOs.Notes;

public sealed class ReadingProgressResponse
{
    public Guid NoteId { get; set; }
    public int Percent { get; set; }
    public DateTime LastReadAtUtc { get; set; }
}
