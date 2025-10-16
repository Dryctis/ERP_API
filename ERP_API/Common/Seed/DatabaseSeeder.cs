using ERP_API.Common.Constants;
using ERP_API.Data;
using ERP_API.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERP_API.Common.Seed;


public static class DatabaseSeeder
{
  
    public static async Task SeedAsync(AppDbContext db)
    {
       
        if (await db.Roles.AnyAsync())
            return;

        await SeedRolesAsync(db);
        await SeedDefaultAdminAsync(db);

        await db.SaveChangesAsync();
    }

    
    private static async Task SeedRolesAsync(AppDbContext db)
    {
        var roles = new List<Role>
        {
            new() { Name = RoleConstants.Admin },
            new() { Name = RoleConstants.User }
        };

        await db.Roles.AddRangeAsync(roles);
    }

   
    private static async Task SeedDefaultAdminAsync(AppDbContext db)
    {
        var adminRole = await db.Roles
            .FirstAsync(r => r.Name == RoleConstants.Admin);

        var adminUser = new User
        {
            Email = "admin@demo.test",
            FullName = "Administrator",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
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