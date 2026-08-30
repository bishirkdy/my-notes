using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechNotes.Domain.Entities.Notes;

namespace TechNotes.Persistence.Configurations.Notes
{
    // Configures the relationship between users and their favorite notes.
    public sealed class FavoriteConfiguration : IEntityTypeConfiguration<Favorite>
    {
        public void Configure(EntityTypeBuilder<Favorite> builder)
        {
            builder.HasKey(x => x.Id);

            // Connects the favorite to the note.
            builder.HasOne(x => x.Note)
                .WithMany(x => x.Favorites)
                .HasForeignKey(x => x.NoteId)
                .OnDelete(DeleteBehavior.Cascade);

            // Connects the favorite to the user.
            builder.HasOne(x => x.User)
                .WithMany(x => x.Favorites)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Prevents a user from favoriting the same note twice.
            builder.HasIndex(x => new { x.UserId, x.NoteId })
                .IsUnique();
        }
    }
}
