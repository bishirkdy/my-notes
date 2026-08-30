using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechNotes.Domain.Entities.Notes;


namespace TechNotes.Persistence.Configurations.Notes
{
    // Configures file metadata associated with notes.
    public sealed class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
    {
        public void Configure(EntityTypeBuilder<Attachment> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.FileName)
                .IsRequired()
                .HasMaxLength(255);

            builder.Property(x => x.ContentType)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(x => x.StoragePath)
                .IsRequired()
                .HasMaxLength(1000);

            // A note can contain multiple attachments.
            builder.HasOne(x => x.Note)
                .WithMany(x => x.Attachments)
                .HasForeignKey(x => x.NoteId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
