using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TicketSystemWeb.Data;
using TicketSystemWeb.Models.Employee;
using TicketSystemWeb.Models.KanbanBoard;
using TicketSystemWeb.Models.ProjectManagement.Project;

namespace TicketSystemWeb.Data
{
    /// <summary>
    /// The database initializer class.
    /// </summary>
    public static class InitializeDB
    {
        /// <summary>
        /// Seeds the database.
        /// </summary>
        /// <param name="serviceProvider">The service provider.</param>
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

            if (!await dbContext.Projects.AnyAsync())
            {
                var project = new Project
                {
                    Title = "Ticket System Project",
                    Description = "A project for managing tickets with a Kanban board.",
                    ProjectManagerId = "e8d2f7a0-ecf1-4e0a-a3e5-3e3ddedf1b1d"
                };
                dbContext.Projects.Add(project);
                await dbContext.SaveChangesAsync();

                var board = new KanbanBoard
                {
                    ProjectId = project.Id,
                    ProjectName = project.Title
                };
                dbContext.KanbanBoards.Add(board);
                await dbContext.SaveChangesAsync();

                dbContext.KanbanColumns.AddRange(
                    new KanbanColumn { Name = "To Do", Order = 1, KanbanBoardId = board.Id },
                    new KanbanColumn { Name = "In Progress", Order = 2, KanbanBoardId = board.Id },
                    new KanbanColumn { Name = "Done", Order = 3, KanbanBoardId = board.Id },
                    new KanbanColumn { Name = "Backlog", Order = 4, KanbanBoardId = board.Id }
                );
                await dbContext.SaveChangesAsync();

                dbContext.Tickets.AddRange(new Ticket[]
                {
                    new Ticket
                    {
                        Title = "First Ticket",
                        Description = "This is a test ticket.",
                        Status = "To Do",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "Admin",
                        ProjectId = project.Id
                    },
                    new Ticket
                    {
                        Title = "Second Ticket",
                        Description = "Another test ticket.",
                        Status = "In Progress",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "User123",
                        ProjectId = project.Id
                    },
                    new Ticket
                    {
                        Title = "Completed Ticket",
                        Description = "This ticket is done.",
                        Status = "Done",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "Admin",
                        ProjectId = project.Id
                    }
                });
                await dbContext.SaveChangesAsync();
            }
        }
    }
}
