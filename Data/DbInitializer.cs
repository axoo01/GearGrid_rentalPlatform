using GearGrid.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GearGrid.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(ApplicationDbContext context)
    {
        await context.Database.MigrateAsync();

        if (!await context.Categories.AnyAsync())
        {
            context.Categories.AddRange(
                new Category
                {
                    CategoryName = "Audio & Lighting",
                    Description = "Audio equipment and event lighting."
                },
                new Category
                {
                    CategoryName = "Tents & Seating",
                    Description = "Shelter, tables, and seating for events."
                },
                new Category
                {
                    CategoryName = "Power & Generators",
                    Description = "Generators and portable power equipment."
                },
                new Category
                {
                    CategoryName = "Cameras & AV",
                    Description = "Cameras, displays, and audiovisual equipment."
                },
                new Category
                {
                    CategoryName = "Staging & Rigging",
                    Description = "Stage platforms and rigging equipment."
                });

            await context.SaveChangesAsync();
        }

        if (!await context.Users.AnyAsync(user => user.Role == "Admin"))
        {
            var adminUser = new User
            {
                FullName = "System Admin",
                Email = "admin@geargrid.com",
                PhoneNumber = "0780000000",
                Role = "Admin"
            };
            var passwordHasher = new PasswordHasher<User>();
            adminUser.PasswordHash = passwordHasher.HashPassword(adminUser, "Admin@123");

            context.Users.Add(adminUser);
            await context.SaveChangesAsync();
        }
    }
}
