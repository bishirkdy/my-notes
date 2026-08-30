using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechNotes.Domain.Entities.Notes;

namespace TechNotes.Persistence.Configurations.Notes
{
    // Configures note sharing and the permission granted to another user.
    public sealed class NoteShareConfiguration : IEntityTypeConfiguration<NoteShare>
    {
        public void Configure(EntityTypeBuilder<NoteShare> builder)
        {
            builder.HasKey(x => x.Id);

            // Connects the share record to the note.
            builder.HasOne(x => x.Note)
                .WithMany(x => x.Shares)
                .HasForeignKey(x => x.NoteId)
                .OnDelete(DeleteBehavior.Cascade);

            //Connects the share record to the user receiving access.
            builder.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // A user can have only one share record per note.
            builder.HasIndex(x => new { x.NoteId, x.UserId })
                .IsUnique();
        }
    }
}
