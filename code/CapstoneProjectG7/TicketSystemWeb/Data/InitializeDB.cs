using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TicketSystemWeb.Data;
using TicketSystemWeb.Models.Employee;
using TicketSystemWeb.Models.KanbanBoard;
using TicketSystemWeb.Models.ProjectManagement.Group;
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
                await roleManager.CreateAsync(new IdentityRole { Name = adminRole, NormalizedName = "ADMIN" });
            if (!await roleManager.RoleExistsAsync(userRole))
                await roleManager.CreateAsync(new IdentityRole { Name = userRole, NormalizedName = "USER" });
            var adminUser = await CreateUser(userManager, "admin", "admin@example.com", adminRole);
            var user1 = await CreateUser(userManager, "johndoe", "john.doe@example.com", userRole);
            var user2 = await CreateUser(userManager, "janesmith", "jane.smith@example.com", userRole);
            var user3 = await CreateUser(userManager, "michael", "michael@example.com", userRole);
            if (!await dbContext.Groups.AnyAsync())
            {
                var group1 = new Group { Name = "Development Team", ManagerId = adminUser.Id };
                var group2 = new Group { Name = "QA Team", ManagerId = user1.Id };
                var group3 = new Group { Name = "Support Team", ManagerId = user2.Id };
                dbContext.Groups.AddRange(group1, group2, group3);
                await dbContext.SaveChangesAsync();
                dbContext.EmployeeGroups.AddRange(
                    new EmployeeGroup { EmployeeId = user1.Id, GroupId = group1.Id },
                    new EmployeeGroup { EmployeeId = user2.Id, GroupId = group2.Id },
                    new EmployeeGroup { EmployeeId = user3.Id, GroupId = group3.Id },
                    new EmployeeGroup { EmployeeId = adminUser.Id, GroupId = group1.Id }
                );
                await dbContext.SaveChangesAsync();
            }
            if (!await dbContext.Projects.AnyAsync())
            {
                var group1 = new Group { Name = "Development Team", ManagerId = adminUser.Id };
                var group2 = new Group { Name = "QA Team", ManagerId = user1.Id };
                var group3 = new Group { Name = "Support Team", ManagerId = user2.Id };
                dbContext.Groups.AddRange(group1, group2, group3);
                await dbContext.SaveChangesAsync();
                var devTeam = await dbContext.Groups.FirstOrDefaultAsync(g => g.Name == "Development Team");
                var qaTeam = await dbContext.Groups.FirstOrDefaultAsync(g => g.Name == "QA Team");
                var supportTeam = await dbContext.Groups.FirstOrDefaultAsync(g => g.Name == "Support Team");
                var project1 = new Project
                {
                    Title = "Ticket System Project",
                    Description = "A project for managing tickets with a Kanban board.",
                    ProjectManagerId = adminUser.Id
                };

                var project2 = new Project
                {
                    Title = "Bug Tracking System",
                    Description = "A project for tracking and resolving software bugs.",
                    ProjectManagerId = user1.Id
                };
                dbContext.Projects.AddRange(project1, project2);
                await dbContext.SaveChangesAsync();
                var ticketSystemProject = await dbContext.Projects.FirstOrDefaultAsync(p => p.Title == "Ticket System Project");
                var bugTrackingProject = await dbContext.Projects.FirstOrDefaultAsync(p => p.Title == "Bug Tracking System");
                dbContext.ProjectGroups.AddRange(
                    new ProjectGroup { ProjectId = ticketSystemProject.Id, GroupId = devTeam.Id },
                    new ProjectGroup { ProjectId = ticketSystemProject.Id, GroupId = qaTeam.Id },
                    new ProjectGroup { ProjectId = bugTrackingProject.Id, GroupId = supportTeam.Id }
                );
                await dbContext.SaveChangesAsync();
                var board1 = new KanbanBoard { ProjectId = ticketSystemProject.Id, ProjectName = ticketSystemProject.Title };
                var board2 = new KanbanBoard { ProjectId = bugTrackingProject.Id, ProjectName = bugTrackingProject.Title };
                dbContext.KanbanBoards.AddRange(board1, board2);
                await dbContext.SaveChangesAsync();
                var kanbanBoard1 = await dbContext.KanbanBoards.FirstOrDefaultAsync(b => b.ProjectId == ticketSystemProject.Id);
                var kanbanBoard2 = await dbContext.KanbanBoards.FirstOrDefaultAsync(b => b.ProjectId == bugTrackingProject.Id);
                dbContext.KanbanColumns.AddRange(
                    new KanbanColumn { Name = "To Do", Order = 1, KanbanBoardId = kanbanBoard1.Id },
                    new KanbanColumn { Name = "In Progress", Order = 2, KanbanBoardId = kanbanBoard1.Id },
                    new KanbanColumn { Name = "Done", Order = 3, KanbanBoardId = kanbanBoard1.Id },
                    new KanbanColumn { Name = "Backlog", Order = 4, KanbanBoardId = kanbanBoard1.Id },

                    new KanbanColumn { Name = "To Do", Order = 1, KanbanBoardId = kanbanBoard2.Id },
                    new KanbanColumn { Name = "Planned", Order = 2, KanbanBoardId = kanbanBoard2.Id },
                    new KanbanColumn { Name = "Completed", Order = 3, KanbanBoardId = kanbanBoard2.Id },
                    new KanbanColumn { Name = "Backlog", Order = 4, KanbanBoardId = kanbanBoard2.Id }
                );
                await dbContext.SaveChangesAsync();
                dbContext.Tickets.AddRange(
                    new Ticket
                    {
                        Title = "Implement Login Feature",
                        Description = "Develop and integrate login system.",
                        Status = "To Do",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "Admin",
                        ProjectId = ticketSystemProject.Id
                    },
                    new Ticket
                    {
                        Title = "Setup CI/CD Pipeline",
                        Description = "Automate deployment process.",
                        Status = "In Progress",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "JohnDoe",
                        ProjectId = ticketSystemProject.Id
                    },
                    new Ticket
                    {
                        Title = "Refactor API Code",
                        Description = "Improve performance and readability.",
                        Status = "Done",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "JaneSmith",
                        ProjectId = ticketSystemProject.Id
                    }
                );
                dbContext.Tickets.AddRange(
                    new Ticket
                    {
                        Title = "Bug #123 - Fix Login Issue",
                        Description = "Resolve authentication bug affecting users.",
                        Status = "Backlog",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "Michael",
                        ProjectId = bugTrackingProject.Id
                    },
                    new Ticket
                    {
                        Title = "Optimize Database Queries",
                        Description = "Improve slow queries for bug tracking.",
                        Status = "Planned",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "JaneSmith",
                        ProjectId = bugTrackingProject.Id
                    },
                    new Ticket
                    {
                        Title = "Deploy v1.0 Release",
                        Description = "Push stable version to production.",
                        Status = "Completed",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "JohnDoe",
                        ProjectId = bugTrackingProject.Id
                    }
                );
                await dbContext.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Creates a new user if they do not exist.
        /// </summary>
        private static async Task<Employee> CreateUser(UserManager<Employee> userManager, string username, string email, string role)
        {
            var user = await userManager.FindByNameAsync(username);
            if (user == null)
            {
                user = new Employee
                {
                    UserName = username,
                    Email = email,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(user, username);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, role);
                }
            }
            return user;
        }
    }
}
