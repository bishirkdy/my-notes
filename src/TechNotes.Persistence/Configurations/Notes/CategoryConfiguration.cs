using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TechNotes.Domain.Entities.Notes;

namespace TechNotes.Persistence.Configurations.Notes
{
    // Configures reusable note categories owned by users.
    public sealed class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(x => x.Description)
                .HasMaxLength(500);

            builder.Property(x => x.SortOrder)
                .HasDefaultValue(0);

            // A user can create many categories.
            builder.HasOne(x => x.User)
                .WithMany(user => user.Categories)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Parent categories group reusable subjects such as Languages or Frameworks.
            builder.HasOne(x => x.ParentCategory)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            // A category name must be unique for the same user.
            builder.HasIndex(x => new { x.UserId, x.Name })
                .IsUnique();

            builder.HasIndex(x => new { x.UserId, x.ParentCategoryId, x.SortOrder });
        }
    }
}
