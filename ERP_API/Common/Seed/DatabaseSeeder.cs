using ERP_API.Common.Configuration;
using ERP_API.Common.Constants;
using ERP_API.Data;
using ERP_API.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Common.Seed;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext db, AdminSettings adminSettings)
    {
        if (await db.Roles.AnyAsync())
            return;

        await SeedRolesAsync(db);
        await SeedDefaultAdminAsync(db, adminSettings);

        await db.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(AppDbContext db)
    {
        var roles = RoleConstants.AllRoles.Select(roleName => new Role
        {
            Name = roleName
        }).ToList();

        await db.Roles.AddRangeAsync(roles);
    }

    private static async Task SeedDefaultAdminAsync(AppDbContext db, AdminSettings settings)
    {
        var adminRole = await db.Roles
            .FirstAsync(r => r.Name == RoleConstants.Admin);

        var adminUser = new User
        {
            Email = settings.Email,
            FullName = settings.FullName,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(settings.DefaultPassword),
            IsActive = true
        };

        await db.Users.AddAsync(adminUser);

        await db.UserRoles.AddAsync(new UserRole
        {
            User = adminUser,
            Role = adminRole
        });
    }
}