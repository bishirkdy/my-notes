using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechNotes.Domain.Entities.Notes;

namespace TechNotes.Persistence.Configurations.Notes
{

    // Configures historical versions of a note.
    public sealed class NoteVersionConfiguration : IEntityTypeConfiguration<NoteVersion>
    {
        public void Configure(EntityTypeBuilder<NoteVersion> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(200);

            // Structured note content is stored as PostgreSQL JSONB.
            builder.Property(x => x.Content)
                .IsRequired()
                .HasColumnType("jsonb");

            // Each version belongs to one note.
            builder.HasOne(x => x.Note)
                .WithMany(x => x.Versions)
                .HasForeignKey(x => x.NoteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Tracks which user created this version.
            builder.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Each note can have a version number only once.
            builder.HasIndex(x => new { x.NoteId, x.VersionNumber })
                .IsUnique();
        }
    }
}
