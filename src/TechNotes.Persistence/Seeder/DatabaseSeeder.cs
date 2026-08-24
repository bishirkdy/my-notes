using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TechNotes.Application.Interfaces.Services.Auth;
using TechNotes.Application.Settings.Auth;
using TechNotes.Domain.Constants;
using TechNotes.Domain.Entities.Identity;
using TechNotes.Persistence.Data;
namespace TechNotes.Persistence.Seeder;

// Adds the required Admin and User roles, and optionally creates the first administrator account.
public sealed class DatabaseSeeder(AppDbContext dbContext, IPasswordHasher passwordHasher, IOptions<AdminSeedOptions> adminOptions)
{
    public async Task SeedAsync()
    {
        await EnsureRoleAsync(RoleNames.Admin);
        await EnsureRoleAsync(RoleNames.User);

        var admin = adminOptions.Value;
        if (string.IsNullOrWhiteSpace(admin.Password) || admin.Password.StartsWith("CHANGE_ME", StringComparison.Ordinal))
        {
            return;
        }

        var email = admin.Email.Trim().ToLowerInvariant();
        if (await dbContext.Users.AnyAsync(user => user.Email == email))
        {
            return;
        }

        var adminRole = await dbContext.Roles.SingleAsync(role => role.Name == RoleNames.Admin);
        var user = new AppUser
        {
            Email = email,
            UserName = admin.UserName.Trim()
        };
        user.PasswordHash = passwordHasher.HashPassword(admin.Password);
        user.UserRoles.Add(new UserRole { Role = adminRole });

        dbContext.Users.Add(user);
        await dbContext.SaveChangesAsync();
    }

    private async Task EnsureRoleAsync(string roleName)
    {
        if (!await dbContext.Roles.AnyAsync(role => role.Name == roleName))
        {
            dbContext.Roles.Add(new Role { Name = roleName });
            await dbContext.SaveChangesAsync();
        }
    }
}
