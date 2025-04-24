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
            await dbContext.Database.MigrateAsync();
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
            var user4 = await CreateUser(userManager, "alice", "alice@example.com", userRole);
            var user5 = await CreateUser(userManager, "bobby", "bob@example.com", userRole);
            var user6 = await CreateUser(userManager, "charlie", "charlie@example.com", userRole);
            var user7 = await CreateUser(userManager, "danny", "danny@example.com", userRole);
            var user8 = await CreateUser(userManager, "evaa", "eva@example.com", userRole);
            var user9 = await CreateUser(userManager, "frank", "frank@example.com", userRole);
            var user10 = await CreateUser(userManager, "gina", "gina@example.com", userRole);
            await dbContext.SaveChangesAsync();
            adminUser = await userManager.FindByNameAsync("admin");
            user1 = await userManager.FindByNameAsync("johndoe");
            user2 = await userManager.FindByNameAsync("janesmith");
            user3 = await userManager.FindByNameAsync("michael");
            user4 = await userManager.FindByNameAsync("alice");
            user5 = await userManager.FindByNameAsync("bobby");
            user6 = await userManager.FindByNameAsync("charlie");
            user7 = await userManager.FindByNameAsync("danny");
            user8 = await userManager.FindByNameAsync("evaa");
            user9 = await userManager.FindByNameAsync("frank");
            user10 = await userManager.FindByNameAsync("gina");

            if (!await dbContext.Groups.AnyAsync())
            {
                var group1 = new Group { Name = "Development Team", ManagerId = adminUser.Id };
                var group2 = new Group { Name = "QA Team", ManagerId = user1.Id };
                var group3 = new Group { Name = "Support Team", ManagerId = user2.Id };
                var group4 = new Group { Name = "UX Designers", ManagerId = user3.Id };
                var group5 = new Group { Name = "Cyber Security", ManagerId = user4.Id };
                var group6 = new Group { Name = "DevOps Team", ManagerId = user5.Id };
                dbContext.Groups.AddRange(group1, group2, group3, group4, group5, group6);
                await dbContext.SaveChangesAsync();
                dbContext.EmployeeGroups.AddRange(
                    new EmployeeGroup { EmployeeId = user1.Id, GroupId = group1.Id },
                    new EmployeeGroup { EmployeeId = user2.Id, GroupId = group2.Id },
                    new EmployeeGroup { EmployeeId = user3.Id, GroupId = group3.Id },
                    new EmployeeGroup { EmployeeId = user4.Id, GroupId = group4.Id },
                    new EmployeeGroup { EmployeeId = user5.Id, GroupId = group5.Id },
                    new EmployeeGroup { EmployeeId = adminUser.Id, GroupId = group6.Id }
                );
                dbContext.EmployeeGroups.AddRange(
                    new EmployeeGroup { EmployeeId = user6.Id, GroupId = group1.Id },
                    new EmployeeGroup { EmployeeId = user6.Id, GroupId = group2.Id },
                    new EmployeeGroup { EmployeeId = user7.Id, GroupId = group1.Id },
                    new EmployeeGroup { EmployeeId = user8.Id, GroupId = group4.Id },
                    new EmployeeGroup { EmployeeId = user9.Id, GroupId = group5.Id }
                );
                await dbContext.SaveChangesAsync();
            }
            if (!await dbContext.Projects.AnyAsync())
            {
                var devTeam = await dbContext.Groups.FirstOrDefaultAsync(g => g.Name == "Development Team");
                var qaTeam = await dbContext.Groups.FirstOrDefaultAsync(g => g.Name == "QA Team");
                var supportTeam = await dbContext.Groups.FirstOrDefaultAsync(g => g.Name == "Support Team");
                var uxTeam = await dbContext.Groups.FirstOrDefaultAsync(g => g.Name == "UX Designers");
                var securityTeam = await dbContext.Groups.FirstOrDefaultAsync(g => g.Name == "Cyber Security");
                var devOpsTeam = await dbContext.Groups.FirstOrDefaultAsync(g => g.Name == "DevOps Team");
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
                var project3 = new Project
                {
                    Title = "E-commerce Platform",
                    Description = "An online marketplace with payment integration.",
                    ProjectManagerId = user2.Id
                };
                var project4 = new Project
                {
                    Title = "Internal Tools",
                    Description = "Internal management tools accessible to DevOps only.",
                    ProjectManagerId = user5.Id
                };
                dbContext.Projects.Add(project4);
                await dbContext.SaveChangesAsync();

                var toolsProject = await dbContext.Projects.FirstOrDefaultAsync(p => p.Title == "Internal Tools");
                dbContext.ProjectGroups.Add(
                    new ProjectGroup { ProjectId = toolsProject.Id, GroupId = devOpsTeam.Id }
                );
                await dbContext.SaveChangesAsync();
                dbContext.Projects.AddRange(project1, project2, project3);
                await dbContext.SaveChangesAsync();
                var ticketSystemProject = await dbContext.Projects.FirstOrDefaultAsync(p => p.Title == "Ticket System Project");
                var bugTrackingProject = await dbContext.Projects.FirstOrDefaultAsync(p => p.Title == "Bug Tracking System");
                var ecommerceProject = await dbContext.Projects.FirstOrDefaultAsync(p => p.Title == "E-commerce Platform");
                dbContext.ProjectGroups.AddRange(
                    new ProjectGroup { ProjectId = ticketSystemProject.Id, GroupId = devTeam.Id, IsApproved = true },
                    new ProjectGroup { ProjectId = ticketSystemProject.Id, GroupId = qaTeam.Id, IsApproved = true },
                    new ProjectGroup { ProjectId = bugTrackingProject.Id, GroupId = supportTeam.Id, IsApproved = true },
                    new ProjectGroup { ProjectId = bugTrackingProject.Id, GroupId = uxTeam.Id, IsApproved = true },
                    new ProjectGroup { ProjectId = ecommerceProject.Id, GroupId = securityTeam.Id, IsApproved = true },
                    new ProjectGroup { ProjectId = ecommerceProject.Id, GroupId = devOpsTeam.Id, IsApproved = true }
                );
                await dbContext.SaveChangesAsync();
                var board1 = new KanbanBoard { ProjectId = ticketSystemProject.Id, ProjectName = ticketSystemProject.Title };
                var board2 = new KanbanBoard { ProjectId = bugTrackingProject.Id, ProjectName = bugTrackingProject.Title };
                var board3 = new KanbanBoard { ProjectId = ecommerceProject.Id, ProjectName = ecommerceProject.Title }; 
                var board4 = new KanbanBoard { ProjectId = toolsProject.Id, ProjectName = toolsProject.Title };
                dbContext.KanbanBoards.AddRange(board1, board2, board3, board4);
                await dbContext.SaveChangesAsync();
                var kanbanBoard1 = await dbContext.KanbanBoards.FirstOrDefaultAsync(b => b.ProjectId == ticketSystemProject.Id);
                var kanbanBoard2 = await dbContext.KanbanBoards.FirstOrDefaultAsync(b => b.ProjectId == bugTrackingProject.Id);
                var kanbanBoard3 = await dbContext.KanbanBoards.FirstOrDefaultAsync(b => b.ProjectId == ecommerceProject.Id);
                var kanbanBoard4 = await dbContext.KanbanBoards.FirstOrDefaultAsync(b => b.ProjectId == toolsProject.Id);
                dbContext.KanbanColumns.AddRange(
                    new KanbanColumn { Name = "To Do", Order = 1, KanbanBoardId = kanbanBoard1.Id },
                    new KanbanColumn { Name = "In Progress", Order = 2, KanbanBoardId = kanbanBoard1.Id },
                    new KanbanColumn { Name = "Done", Order = 3, KanbanBoardId = kanbanBoard1.Id },
                    new KanbanColumn { Name = "Backlog", Order = 4, KanbanBoardId = kanbanBoard1.Id },
                    new KanbanColumn { Name = "To Do", Order = 1, KanbanBoardId = kanbanBoard2.Id },
                    new KanbanColumn { Name = "Planned", Order = 2, KanbanBoardId = kanbanBoard2.Id },
                    new KanbanColumn { Name = "Completed", Order = 3, KanbanBoardId = kanbanBoard2.Id },
                    new KanbanColumn { Name = "Backlog", Order = 4, KanbanBoardId = kanbanBoard2.Id },
                    new KanbanColumn { Name = "To Do", Order = 1, KanbanBoardId = kanbanBoard3.Id },
                    new KanbanColumn { Name = "Review", Order = 2, KanbanBoardId = kanbanBoard3.Id },
                    new KanbanColumn { Name = "Testing", Order = 3, KanbanBoardId = kanbanBoard3.Id },
                    new KanbanColumn { Name = "Deployment", Order = 4, KanbanBoardId = kanbanBoard3.Id },
                    new KanbanColumn { Name = "Planning", Order = 1, KanbanBoardId = kanbanBoard4.Id },
                    new KanbanColumn { Name = "Execution", Order = 2, KanbanBoardId = kanbanBoard4.Id },
                    new KanbanColumn { Name = "Wrap-Up", Order = 3, KanbanBoardId = kanbanBoard4.Id }
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
                        Title = "Bug #123 - Fix Login Issue",
                        Description = "Resolve authentication bug affecting users.",
                        Status = "Backlog",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "Michael",
                        ProjectId = bugTrackingProject.Id
                    },
                    new Ticket
                    {
                        Title = "Integrate Stripe Payments",
                        Description = "Implement payment gateway for the e-commerce platform.",
                        Status = "Review",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "Alice",
                        ProjectId = ecommerceProject.Id
                    },
                    new Ticket
                    {
                        Title = "Unassigned Task",
                        Description = "No one is responsible yet.",
                        Status = "To Do",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "System",
                        ProjectId = toolsProject.Id
                    },
                    new Ticket
                    {
                        Title = "Strange Status Ticket",
                        Description = "Has an unrecognized status to test UI resilience.",
                        Status = "WTF?",
                        CreatedAt = DateTime.UtcNow,
                        CreatedBy = "Charlie",
                        ProjectId = ecommerceProject.Id
                    }
                );
                await dbContext.SaveChangesAsync();
                var allColumns = await dbContext.KanbanColumns.ToListAsync();
                var devGroup = await dbContext.Groups.FirstOrDefaultAsync(g => g.Name == "Development Team");
                var qaGroup = await dbContext.Groups.FirstOrDefaultAsync(g => g.Name == "QA Team");
                var supportGroup = await dbContext.Groups.FirstOrDefaultAsync(g => g.Name == "Support Team");
                var uxGroup = await dbContext.Groups.FirstOrDefaultAsync(g => g.Name == "UX Designers");
                var secGroup = await dbContext.Groups.FirstOrDefaultAsync(g => g.Name == "Cyber Security");
                var devOpsGroup = await dbContext.Groups.FirstOrDefaultAsync(g => g.Name == "DevOps Team");
                dbContext.ColumnGroupAccesses.AddRange(
                    new ColumnGroupAccess { KanbanColumnId = allColumns.First(c => c.Name == "To Do" && c.KanbanBoard.ProjectId == ticketSystemProject.Id).Id, GroupId = devGroup.Id },
                    new ColumnGroupAccess { KanbanColumnId = allColumns.First(c => c.Name == "In Progress" && c.KanbanBoard.ProjectId == ticketSystemProject.Id).Id, GroupId = devGroup.Id },
                    new ColumnGroupAccess { KanbanColumnId = allColumns.First(c => c.Name == "Done" && c.KanbanBoard.ProjectId == ticketSystemProject.Id).Id, GroupId = qaGroup.Id },
                    new ColumnGroupAccess { KanbanColumnId = allColumns.First(c => c.Name == "Backlog" && c.KanbanBoard.ProjectId == ticketSystemProject.Id).Id, GroupId = devGroup.Id },
                    new ColumnGroupAccess { KanbanColumnId = allColumns.First(c => c.Name == "To Do" && c.KanbanBoard.ProjectId == bugTrackingProject.Id).Id, GroupId = supportGroup.Id },
                    new ColumnGroupAccess { KanbanColumnId = allColumns.First(c => c.Name == "Planned" && c.KanbanBoard.ProjectId == bugTrackingProject.Id).Id, GroupId = supportGroup.Id },
                    new ColumnGroupAccess { KanbanColumnId = allColumns.First(c => c.Name == "Completed" && c.KanbanBoard.ProjectId == bugTrackingProject.Id).Id, GroupId = uxGroup.Id },
                    new ColumnGroupAccess { KanbanColumnId = allColumns.First(c => c.Name == "Backlog" && c.KanbanBoard.ProjectId == bugTrackingProject.Id).Id, GroupId = supportGroup.Id },
                    new ColumnGroupAccess { KanbanColumnId = allColumns.First(c => c.Name == "To Do" && c.KanbanBoard.ProjectId == ecommerceProject.Id).Id, GroupId = secGroup.Id },
                    new ColumnGroupAccess { KanbanColumnId = allColumns.First(c => c.Name == "Review" && c.KanbanBoard.ProjectId == ecommerceProject.Id).Id, GroupId = secGroup.Id },
                    new ColumnGroupAccess { KanbanColumnId = allColumns.First(c => c.Name == "Testing" && c.KanbanBoard.ProjectId == ecommerceProject.Id).Id, GroupId = devOpsGroup.Id },
                    new ColumnGroupAccess { KanbanColumnId = allColumns.First(c => c.Name == "Deployment" && c.KanbanBoard.ProjectId == ecommerceProject.Id).Id, GroupId = devOpsGroup.Id }
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
                if (!result.Succeeded)
                {
                    throw new Exception($"Failed to create user {username}: {string.Join(", ", result.Errors.Select(e => e.Description))}");
                }
                await userManager.AddToRoleAsync(user, role);
            }
            return user ?? throw new Exception($"User {username} could not be created.");
        }
    }
}
