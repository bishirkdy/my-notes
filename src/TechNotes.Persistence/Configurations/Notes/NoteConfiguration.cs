using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechNotes.Domain.Entities.Notes;

namespace TechNotes.Persistence.Configurations.Notes
{
    // Configures the Note entity and its database relationships.
    public sealed class NoteConfiguration : IEntityTypeConfiguration<Note>
    {
        public void Configure(EntityTypeBuilder<Note> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Title)
    .           IsRequired()
                .HasMaxLength(200);

            // A user can own many notes.
            builder.HasOne(x => x.User)
                .WithMany(x => x.Notes)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // A folder can contain many notes.
            builder.HasOne(x => x.Folder)
                .WithMany(x => x.Notes)
                .HasForeignKey(x => x.FolderId)
                .OnDelete(DeleteBehavior.SetNull);

            // A category can contain many notes.
            builder.HasOne(x => x.Category)
                .WithMany(x => x.Notes)
                .HasForeignKey(x => x.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // A note can contain many sections.
            builder.HasMany(x => x.Sections)
                .WithOne(x => x.Note)
                .HasForeignKey(x => x.NoteId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
