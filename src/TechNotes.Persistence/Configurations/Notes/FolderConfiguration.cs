
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechNotes.Domain.Entities.Notes;

namespace TechNotes.Persistence.Configurations.Notes
{
    // Configures folders and their optional parent-child hierarchy.
    public sealed class FolderConfiguration : IEntityTypeConfiguration<Folder>
    {
        public void Configure(EntityTypeBuilder<Folder> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(150);

            // A user owns many folders.
            builder.HasOne(x => x.User)
                .WithMany(x => x.Folders)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // A folder can optionally have a parent folder.
            builder.HasOne(x => x.ParentFolder)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentFolderId)
                .OnDelete(DeleteBehavior.Restrict);

            // Prevents duplicate folder names for the same user.
            builder.HasIndex(x => new { x.UserId, x.Name })
                .IsUnique();
        }
    }
}
