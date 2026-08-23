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
    public DbSet<Folder> Folders => Set<Folder>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<Note> Notes => Set<Note>();
    public DbSet<NoteTag> NoteTags => Set<NoteTag>();
    public DbSet<NoteShare> NoteShares => Set<NoteShare>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<Favorite> Favorites => Set<Favorite>();
    public DbSet<NoteVersion> NoteVersions => Set<NoteVersion>();

    // User notification table.
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

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

        modelBuilder.Entity<Folder>(entity =>
        {
            entity.HasIndex(folder => new { folder.OwnerId, folder.ParentFolderId, folder.Name }).IsUnique();
            entity.Property(folder => folder.Name).HasMaxLength(100);
            entity.HasOne(folder => folder.Owner)
                .WithMany(user => user.Folders)
                .HasForeignKey(folder => folder.OwnerId);
            // A folder cannot delete its child folders automatically.
            entity.HasOne(folder => folder.ParentFolder)
                .WithMany(folder => folder.ChildFolders)
                .HasForeignKey(folder => folder.ParentFolderId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasIndex(category => new { category.OwnerId, category.Name }).IsUnique();
            entity.Property(category => category.Name).HasMaxLength(100);
            entity.Property(category => category.Color).HasMaxLength(20);
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasIndex(tag => new { tag.OwnerId, tag.Name }).IsUnique();
            entity.Property(tag => tag.Name).HasMaxLength(50);
        });

        modelBuilder.Entity<Note>(entity =>
        {
            entity.HasIndex(note => new { note.OwnerId, note.UpdatedAtUtc });
            entity.Property(note => note.Title).HasMaxLength(250);
            // Removing a folder or category leaves its notes in place without that reference.
            entity.HasOne(note => note.Folder)
                .WithMany(folder => folder.Notes)
                .HasForeignKey(note => note.FolderId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasOne(note => note.Category)
                .WithMany(category => category.Notes)
                .HasForeignKey(note => note.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<NoteTag>(entity =>
        {
            entity.HasKey(noteTag => new { noteTag.NoteId, noteTag.TagId });
            entity.HasOne(noteTag => noteTag.Note)
                .WithMany(note => note.NoteTags)
                .HasForeignKey(noteTag => noteTag.NoteId);
            entity.HasOne(noteTag => noteTag.Tag)
                .WithMany(tag => tag.NoteTags)
                .HasForeignKey(noteTag => noteTag.TagId);
        });

        modelBuilder.Entity<NoteShare>(entity =>
        {
            entity.HasIndex(share => new { share.NoteId, share.RecipientId }).IsUnique();
            entity.HasOne(share => share.Note)
                .WithMany(note => note.Shares)
                .HasForeignKey(share => share.NoteId);
            // Prevents deleting a recipient user while a sharing record still refers to that user.
            entity.HasOne(share => share.Recipient)
                .WithMany()
                .HasForeignKey(share => share.RecipientId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.Property(comment => comment.Content).HasMaxLength(5_000);
            // Replies remain protected from accidental cascading deletes.
            entity.HasOne(comment => comment.ParentComment)
                .WithMany(comment => comment.Replies)
                .HasForeignKey(comment => comment.ParentCommentId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Attachment>(entity =>
        {
            entity.Property(attachment => attachment.FileName).HasMaxLength(255);
            entity.Property(attachment => attachment.StoragePath).HasMaxLength(1_024);
            entity.Property(attachment => attachment.ContentType).HasMaxLength(255);
            entity.HasOne(attachment => attachment.UploadedBy)
                .WithMany()
                .HasForeignKey(attachment => attachment.UploadedById)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Favorite>(entity =>
        {
            entity.HasIndex(favorite => new { favorite.UserId, favorite.NoteId }).IsUnique();
        });

        modelBuilder.Entity<NoteVersion>(entity =>
        {
            entity.HasIndex(version => new { version.NoteId, version.VersionNumber }).IsUnique();
            entity.Property(version => version.Title).HasMaxLength(250);
            entity.HasOne(version => version.CreatedBy)
                .WithMany()
                .HasForeignKey(version => version.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);
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
