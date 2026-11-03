using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechNotes.Domain.Entities.Notes;

namespace TechNotes.Persistence.Configurations.Notes;

public sealed class ReadingProgressConfiguration : IEntityTypeConfiguration<ReadingProgress>
{
    public void Configure(EntityTypeBuilder<ReadingProgress> builder)
    {
        builder.HasKey(progress => progress.Id);

        builder.HasOne(progress => progress.User)
            .WithMany(user => user.ReadingProgress)
            .HasForeignKey(progress => progress.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(progress => progress.Note)
            .WithMany(note => note.ReadingProgress)
            .HasForeignKey(progress => progress.NoteId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(progress => new { progress.UserId, progress.NoteId })
            .IsUnique();

        builder.HasIndex(progress => new { progress.UserId, progress.LastReadAtUtc });
    }
}
