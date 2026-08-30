using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechNotes.Domain.Entities.Notes;

namespace TechNotes.Persistence.Configurations.Notes
{
    // Configures flexible content blocks such as definitions, tables, examples, and code.
    public sealed class NoteContentConfiguration : IEntityTypeConfiguration<NoteContent>
    {
        public void Configure(EntityTypeBuilder<NoteContent> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Type)
                .IsRequired();

            builder.Property(x => x.SortOrder)
                .IsRequired();

            // Stores normal text content when the block does not require JSON.
            builder.Property(x => x.Content);

            // Stores structured data such as tables, lists, and graphs.
            // PostgreSQL stores this efficiently as JSONB.
            builder.Property(x => x.Data)
                .HasColumnType("jsonb");

            // Stores optional rendering information.
            builder.Property(x => x.Metadata)
                .HasColumnType("jsonb");

            // Each content block belongs to one section.
            builder.HasOne(x => x.Section)
                .WithMany(x => x.Contents)
                .HasForeignKey(x => x.SectionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}