using Microsoft.EntityFrameworkCore;
using TechNotes.Domain.Entities.Identity;
using TechNotes.Domain.Entities.Notes;
using TechNotes.Domain.Entities.Notifications;

namespace TechNotes.Persistence.Data;

// The EF Core database context. Each DbSet below becomes a database table after a migration is applied.
public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    // Identity and access tables.
    public DbSet<AppUser> Users => Set<AppUser>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Note-management tables.
    public DbSet<Note> Notes => Set<Note>();                            // Stores the main notes.
    public DbSet<NoteSection> NoteSections => Set<NoteSection>();       // Stores headings and nested subheadings.
    public DbSet<NoteContent> NoteContents => Set<NoteContent>();       // Stores dynamic content blocks such as definitions, tables, examples, and code.
    public DbSet<Folder> Folders => Set<Folder>();                      // Stores user folders.
    public DbSet<Category> Categories => Set<Category>();               // Stores note categories.
    public DbSet<Tag> Tags => Set<Tag>();                               // Stores reusable tags.
    public DbSet<NoteTag> NoteTags => Set<NoteTag>();                   // Stores the many-to-many relationship between notes and tags.
    public DbSet<NoteShare> NoteShares => Set<NoteShare>();             // Stores note sharing permissions.
    public DbSet<Comment> Comments => Set<Comment>();                   // Stores comments and replies.
    public DbSet<Attachment> Attachments => Set<Attachment>();          // Stores metadata for files attached to notes.
    public DbSet<Favorite> Favorites => Set<Favorite>();                // Stores notes marked as favorites by users.
    public DbSet<NoteVersion> NoteVersions => Set<NoteVersion>();       // Stores historical versions of notes.
    public DbSet<Notification> Notifications => Set<Notification>();    // User notification table.


    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Automatically applies all IEntityTypeConfiguration classes
        // from the Persistence assembly.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Adds constraints, indexes, and relationship rules that EF Core uses when generating the database.
        modelBuilder.Entity<AppUser>(entity =>
        {
            entity.HasIndex(user => user.Email).IsUnique();
            entity.HasIndex(user => user.UserName).IsUnique();
            entity.Property(user => user.Email).HasMaxLength(256);
            entity.Property(user => user.UserName).HasMaxLength(100);
            entity.Property(user => user.PasswordHash).HasMaxLength(512);
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasIndex(role => role.Name).IsUnique();
            entity.Property(role => role.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(userRole => new { userRole.UserId, userRole.RoleId });
            entity.HasOne(userRole => userRole.User)
                .WithMany(user => user.UserRoles)
                .HasForeignKey(userRole => userRole.UserId);
            entity.HasOne(userRole => userRole.Role)
                .WithMany(role => role.UserRoles)
                .HasForeignKey(userRole => userRole.RoleId);
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.HasIndex(token => token.Token).IsUnique();
            entity.Property(token => token.Token).HasMaxLength(512);
            entity.HasOne(token => token.User)
                .WithMany(user => user.RefreshTokens)
                .HasForeignKey(token => token.UserId);
        });

      
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.Property(notification => notification.Message).HasMaxLength(1_000);
            entity.HasIndex(notification => new { notification.UserId, notification.IsRead, notification.CreatedAtUtc });
            entity.HasOne(notification => notification.Note)
                .WithMany()
                .HasForeignKey(notification => notification.NoteId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}
