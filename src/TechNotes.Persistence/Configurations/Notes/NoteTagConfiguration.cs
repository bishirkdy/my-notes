using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechNotes.Domain.Entities.Notes;

namespace TechNotes.Persistence.Configurations.Notes
{
    // Configures the many-to-many relationship between notes and tags.
    public sealed class NoteTagConfiguration : IEntityTypeConfiguration<NoteTag>
    {
        public void Configure(EntityTypeBuilder<NoteTag> builder)
        {
            builder.HasKey(x => x.Id);

            // Connects each note-tag record to exactly one note.
            builder.HasOne(x => x.Note)
                .WithMany(x => x.NoteTags)
                .HasForeignKey(x => x.NoteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Connects each note-tag record to exactly one tag.
            builder.HasOne(x => x.Tag)
                .WithMany(x => x.NoteTags)
                .HasForeignKey(x => x.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            // Prevents the same tag from being attached to a note twice.
            builder.HasIndex(x => new { x.NoteId, x.TagId })
                .IsUnique();
        }
    }
}
