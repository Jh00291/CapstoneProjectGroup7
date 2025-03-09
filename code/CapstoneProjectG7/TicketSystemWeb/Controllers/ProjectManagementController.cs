using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSystemWeb.Data;
using TicketSystemWeb.Models.Employee;
using TicketSystemWeb.Models.KanbanBoard;
using TicketSystemWeb.Models.ProjectManagement.Group;
using TicketSystemWeb.Models.ProjectManagement.Project;
using TicketSystemWeb.ViewModels;

namespace TicketSystemWeb.Controllers
{
    /// <summary>
    /// the project management controller
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    [Authorize]
    public class ProjectManagementController : Controller
    {
        private readonly TicketDBContext _context;
        private readonly UserManager<Employee> _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectManagementController"/> class.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="userManager">The user manager.</param>
        public ProjectManagementController(TicketDBContext context, UserManager<Employee> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Managements this instance.
        /// </summary>
        /// <returns>the management page</returns>
        [HttpGet]
        public async Task<IActionResult> Management()
        {
            var projects = await _context.Projects
                .Include(p => p.ProjectManager)
                .Include(p => p.ProjectGroups)
                .ThenInclude(pg => pg.Group)
                .ToListAsync();
            var groups = await _context.Groups.Include(g => g.Manager).ToListAsync();
            var employees = await _context.Users.ToListAsync();
            var user = await _userManager.GetUserAsync(User);
            bool isAdmin = await _userManager.IsInRoleAsync(user, "admin");
            bool isGroupManager = _context.Groups.Any(g => g.ManagerId == user.Id);
            bool isProjectManager = _context.Projects.Any(p => p.ProjectManagerId == user.Id);
            var viewModel = new ManagementViewModel
            {
                Projects = projects,
                Groups = groups,
                AllEmployees = employees,
                CanAddProject = isAdmin || isGroupManager,
                CanManageGroups = isAdmin || isGroupManager
            };
            return View("Management", viewModel);
        }

        /// <summary>
        /// Creates the project.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>Redirects to the management page.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProject(AddProjectViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid project details.");
            }
            bool projectExists = await _context.Projects.AnyAsync(p => p.Title == model.Title);
            if (projectExists)
            {
                return BadRequest("A project with this title already exists.");
            }
            var projectManager = await _context.Users.FindAsync(model.ProjectManagerId);
            if (projectManager == null)
            {
                return BadRequest("Invalid Project Manager");
            }
            var project = new Project
            {
                Title = model.Title,
                Description = model.Description,
                ProjectManagerId = projectManager.Id,
                ProjectManager = projectManager
            };
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            var kanbanBoard = new KanbanBoard
            {
                ProjectId = project.Id,
                ProjectName = project.Title
            };
            _context.KanbanBoards.Add(kanbanBoard);
            await _context.SaveChangesAsync();
            _context.KanbanColumns.AddRange(
                new KanbanColumn { Name = "To Do", Order = 1, KanbanBoardId = kanbanBoard.Id },
                new KanbanColumn { Name = "In Progress", Order = 2, KanbanBoardId = kanbanBoard.Id },
                new KanbanColumn { Name = "Done", Order = 3, KanbanBoardId = kanbanBoard.Id }
            );
            await _context.SaveChangesAsync();
            if (model.GroupIds != null && model.GroupIds.Any())
            {
                foreach (var groupId in model.GroupIds)
                {
                    _context.ProjectGroups.Add(new ProjectGroup
                    {
                        ProjectId = project.Id,
                        GroupId = groupId
                    });
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Management");
        }

        /// <summary>
        /// Deletes the project.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <returns>to the management page</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProject(int projectId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return NotFound("Project not found.");
            bool isAdmin = await _userManager.IsInRoleAsync(user, "admin");
            if (!isAdmin && project.ProjectManagerId != user.Id)
            {
                return Forbid();
            }
            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();
            return RedirectToAction("Management", "ProjectManagement");
        }

        /// <summary>
        /// Updates the project.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>to the management page</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProject(EditProjectViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid project details.");
            }
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();
            var project = await _context.Projects
                .Include(p => p.ProjectGroups)
                .FirstOrDefaultAsync(p => p.Id == model.ProjectId);
            if (project == null) return NotFound();
            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (!isAdmin && project.ProjectManagerId != user.Id)
            {
                return Forbid();
            }
            project.Title = model.Title;
            project.Description = model.Description;
            if (model.ProjectManagerId != project.ProjectManagerId)
            {
                var newManager = await _context.Users.FindAsync(model.ProjectManagerId);
                if (newManager != null)
                {
                    project.ProjectManagerId = newManager.Id;
                    project.ProjectManager = newManager;
                }
            }
            var existingGroupIds = project.ProjectGroups.Select(pg => pg.GroupId).ToList();
            var newGroupIds = model.SelectedGroupIds ?? new List<int>();
            _context.ProjectGroups.RemoveRange(
                project.ProjectGroups.Where(pg => !newGroupIds.Contains(pg.GroupId))
            );
            foreach (var groupId in newGroupIds.Except(existingGroupIds))
            {
                _context.ProjectGroups.Add(new ProjectGroup
                {
                    ProjectId = project.Id,
                    GroupId = groupId
                });
            }
            _context.Projects.Update(project);
            await _context.SaveChangesAsync();
            return RedirectToAction("Management", "ProjectManagement");
        }

        /// <summary>
        /// Edits the project.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <returns>to the management page</returns>
        [HttpGet]
        public async Task<IActionResult> EditProject(int projectId)
        {
            var project = await _context.Projects
                .Include(p => p.ProjectGroups)
                .ThenInclude(pg => pg.Group)
                .FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null) return NotFound();
            var allGroups = await _context.Groups.ToListAsync();
            var allEmployees = await _context.Users.ToListAsync();
            var viewModel = new EditProjectViewModel
            {
                ProjectId = project.Id,
                Title = project.Title,
                Description = project.Description,
                ProjectManagerId = project.ProjectManagerId,
                SelectedGroupIds = project.ProjectGroups.Select(pg => pg.GroupId).ToList(),
                AllGroups = allGroups,
                AllEmployees = allEmployees
            };
            return PartialView("Projects/EditProject", viewModel);
        }

        /// <summary>
        /// Creates the group.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>to the management page</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroup(AddGroupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid group data.");
            }
            bool groupExists = await _context.Groups.AnyAsync(g => g.Name == model.Name);
            if (groupExists)
            {
                return BadRequest("A group with this name already exists.");
            }
            var manager = await _context.Users.FindAsync(model.SelectedManagerId);
            if (manager == null)
            {
                return BadRequest("Please select a Manager.");
            }
            var group = new Group
            {
                Name = model.Name,
                ManagerId = model.SelectedManagerId,
                Manager = manager
            };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();
            if (model.SelectedEmployeeIds != null && model.SelectedEmployeeIds.Any())
            {
                foreach (var employeeId in model.SelectedEmployeeIds)
                {
                    var employee = await _context.Users.FindAsync(employeeId);
                    if (employee != null)
                    {
                        _context.EmployeeGroups.Add(new EmployeeGroup
                        {
                            EmployeeId = employeeId,
                            GroupId = group.Id
                        });
                    }
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Management");
        }

        /// <summary>
        /// Edits the group.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>to the management page</returns>
        [HttpGet]
        public async Task<IActionResult> EditGroup(int id)
        {
            var group = await _context.Groups
                .Include(g => g.Manager)
                .Include(g => g.EmployeeGroups)
                .ThenInclude(eg => eg.Employee)
                .FirstOrDefaultAsync(g => g.Id == id);
            if (group == null) return NotFound();
            var employees = await _context.Users.ToListAsync();
            var viewModel = new EditGroupViewModel
            {
                GroupId = group.Id,
                Name = group.Name,
                SelectedManagerId = group.ManagerId,
                SelectedEmployeeIds = group.EmployeeGroups.Select(eg => eg.EmployeeId).ToList(),
                AllEmployees = employees
            };
            return PartialView("EditGroup", viewModel);
        }

        /// <summary>
        /// Updates the group.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>to the management page</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGroup(EditGroupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid group details.");
            }
            var group = await _context.Groups.FindAsync(model.GroupId);
            if (group == null) return NotFound();
            bool isAdmin = User.IsInRole("admin");
            bool isGroupManager = group.ManagerId == _userManager.GetUserId(User);
            if (!isAdmin && !isGroupManager)
            {
                return Forbid();
            }
            group.Name = model.Name;
            group.ManagerId = model.SelectedManagerId;
            _context.EmployeeGroups.RemoveRange(
                _context.EmployeeGroups.Where(eg => eg.GroupId == model.GroupId)
            );
            foreach (var employeeId in model.SelectedEmployeeIds)
            {
                _context.EmployeeGroups.Add(new EmployeeGroup
                {
                    GroupId = model.GroupId,
                    EmployeeId = employeeId
                });
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Management");
        }

        /// <summary>
        /// Deletes the group.
        /// </summary>
        /// <param name="groupId">The group identifier.</param>
        /// <returns>to the management page</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGroup(int groupId)
        {
            var group = await _context.Groups
                .Include(g => g.EmployeeGroups)
                .FirstOrDefaultAsync(g => g.Id == groupId);
            if (group == null) return NotFound("Group not found.");
            var user = await _userManager.GetUserAsync(User);
            bool isAdmin = await _userManager.IsInRoleAsync(user, "admin");
            bool isGroupManager = group.ManagerId == user.Id;
            if (!isAdmin && !isGroupManager)
            {
                return Forbid();
            }
            _context.EmployeeGroups.RemoveRange(group.EmployeeGroups);
            _context.Groups.Remove(group);
            await _context.SaveChangesAsync();
            return RedirectToAction("Management");
        }
    }
}
