using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechNotes.Domain.Entities.Notes;

namespace TechNotes.Persistence.Configurations.Notes
{
    public sealed class NoteSectionConfiguration : IEntityTypeConfiguration<NoteSection>
    {
        // Configures hierarchical headings and subheadings within a note.
        public void Configure(EntityTypeBuilder<NoteSection> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(300);

            builder.Property(x => x.SortOrder)
                .IsRequired();

            // A section belongs to one note.
            builder.HasOne(x => x.Note)
                .WithMany(x => x.Sections)
                .HasForeignKey(x => x.NoteId)
                .OnDelete(DeleteBehavior.Cascade);

            // A section can optionally have another section as its parent.
            // This creates unlimited heading/subheading nesting.
            builder.HasOne(x => x.ParentSection)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentSectionId)
                .OnDelete(DeleteBehavior.Cascade);

            // A section can contain many content blocks.
            builder.HasMany(x => x.Contents)
                .WithOne(x => x.Section)
                .HasForeignKey(x => x.SectionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}