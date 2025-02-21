using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TicketSystemWeb.Models;
using TicketSystemWeb.Models.Employee;

namespace TicketSystemWeb.Data
{
    public static class InitializeDB
    {
        public static async Task SeedDatabase(IServiceProvider serviceProvider)
        {
            var dbContext = serviceProvider.GetRequiredService<TicketDBContext>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<Employee>>();

            await dbContext.Database.EnsureCreatedAsync(); 
            string adminRole = "admin";
            string userRole = "user";

            if (!await roleManager.RoleExistsAsync(adminRole))
                await roleManager.CreateAsync(new IdentityRole { Id = "c1a5a1b6-4c1d-4a6e-b3e4-7a8b6c5d4e3f", Name = adminRole, NormalizedName = "ADMIN" });

            if (!await roleManager.RoleExistsAsync(userRole))
                await roleManager.CreateAsync(new IdentityRole { Id = "f2b3c4d5-6e7f-8g9h-0i1j-2k3l4m5n6o7p", Name = userRole, NormalizedName = "USER" });

            if (await userManager.FindByNameAsync("admin") == null)
            {
                var adminUser = new Employee
                {
                    Id = "e8d2f7a0-ecf1-4e0a-a3e5-3e3ddedf1b1d",
                    UserName = "admin",
                    Email = "admin@example.com",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(adminUser, "admin");

                if (result.Succeeded)
                    await userManager.AddToRoleAsync(adminUser, adminRole);
            }

            if (await userManager.FindByNameAsync("user") == null)
            {
                var normalUser = new Employee
                {
                    Id = "17c18f27-ae29-4d49-b6f8-62f7ec2a2a34",
                    UserName = "user",
                    Email = "user@example.com",
                    EmailConfirmed = true
                };
                var result = await userManager.CreateAsync(normalUser, "user");

                if (result.Succeeded)
                    await userManager.AddToRoleAsync(normalUser, userRole);
            }

            if (!await dbContext.Tickets.AnyAsync())
            {
                dbContext.Tickets.AddRange(new Ticket[]
                {
                    new Ticket
                    {
                        Title = "First Ticket",
                        Description = "This is a test ticket.",
                        Status = "Open",
                        CreatedAt = new DateTime(2024, 2, 4, 12, 5, 0, DateTimeKind.Utc),
                        CreatedBy = "Admin"
                    },
                    new Ticket
                    {
                        Title = "Second Ticket",
                        Description = "Another test ticket.",
                        Status = "In Progress",
                        CreatedAt = new DateTime(2024, 2, 4, 12, 0, 0, DateTimeKind.Utc),
                        CreatedBy = "User123"
                    }
                });

                await dbContext.SaveChangesAsync();
            }
        }
    }
}
