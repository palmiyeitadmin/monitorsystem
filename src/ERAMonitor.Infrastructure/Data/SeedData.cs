using ERAMonitor.Core.Entities;
using Microsoft.EntityFrameworkCore;

namespace ERAMonitor.Infrastructure.Data;

public static class SeedData
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        var organization = await context.Organizations.FirstOrDefaultAsync();
        if (organization == null)
        {
            organization = new Organization
            {
                Id = Guid.NewGuid(),
                Name = "ERA Monitor",
                Slug = "era-monitor",
                IsActive = true,
            };

            context.Organizations.Add(organization);
            await context.SaveChangesAsync();
        }

        if (await context.Users.AnyAsync(u => u.Email == "admin@eramonitor.com"))
        {
            return;
        }

        var adminUser = new User
        {
            OrganizationId = organization.Id,
            Email = "admin@eramonitor.com",
            FullName = "System Administrator",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Role = ERAMonitor.Core.Enums.UserRole.SuperAdmin,
            IsActive = true,
            EmailVerified = true,
            CreatedAt = DateTime.UtcNow
        };

        context.Users.Add(adminUser);
        await context.SaveChangesAsync();
    }
}
