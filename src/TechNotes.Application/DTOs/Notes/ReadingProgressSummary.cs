namespace TechNotes.Application.DTOs.Notes;

public sealed class ReadingProgressSummary
{
    public int Percent { get; set; }
    public DateTime LastReadAtUtc { get; set; }
}
