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
            var groups = await _context.Groups
                .Include(g => g.Manager)
                .Include(g => g.EmployeeGroups)
                .ThenInclude(eg => eg.Employee)
                .ToListAsync();
            var employees = await _context.Users.ToListAsync();
            var user = await _userManager.GetUserAsync(User);
            bool isAdmin = await _userManager.IsInRoleAsync(user, "admin");
            bool isGroupManager = _context.Groups.Any(g => g.ManagerId == user.Id);
            bool isProjectManager = _context.Projects.Any(p => p.ProjectManagerId == user.Id);
            var selectedGroupIds = new List<int>();
            var pendingApprovals = await _context.ProjectGroups
                .Include(pg => pg.Project)
                .Include(pg => pg.Group)
                .Where(pg => !pg.IsApproved && pg.Group.ManagerId == user.Id)
                .ToListAsync();
            var viewModel = new ManagementViewModel
            {
                Projects = projects,
                Groups = groups,
                AllEmployees = employees,
                CanAddProject = isAdmin || isGroupManager,
                CanManageGroups = isAdmin || isGroupManager,
                SelectedGroupIds = selectedGroupIds,
                PendingApprovals = pendingApprovals
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
                    var group = await _context.Groups.Include(g => g.Manager).FirstOrDefaultAsync(g => g.Id == groupId);
                    if (group == null) continue;

                    bool requiresApproval = group.ManagerId != project.ProjectManagerId;

                    _context.ProjectGroups.Add(new ProjectGroup
                    {
                        ProjectId = project.Id,
                        GroupId = groupId,
                        IsApproved = !requiresApproval // Auto-approve if same manager, else require approval
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
            var newGroupIds = model.SelectedGroupIds ?? new List<int>();
            var existingGroupIds = project.ProjectGroups.Select(pg => pg.GroupId).ToList();
            var groupsToRemove = project.ProjectGroups
                .Where(pg => !newGroupIds.Contains(pg.GroupId))
                .ToList();
            _context.ProjectGroups.RemoveRange(groupsToRemove);
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
            var group = await _context.Groups
                .Include(g => g.EmployeeGroups)
                .FirstOrDefaultAsync(g => g.Id == model.GroupId);
            if (group == null) return NotFound();
            bool isAdmin = User.IsInRole("admin");
            bool isGroupManager = group.ManagerId == _userManager.GetUserId(User);
            if (!isAdmin && !isGroupManager)
            {
                return Forbid();
            }
            group.Name = model.Name;
            group.ManagerId = model.SelectedManagerId;
            var newEmployeeIds = model.SelectedEmployeeIds ?? new List<string>();
            var existingEmployeeIds = group.EmployeeGroups.Select(eg => eg.EmployeeId).ToList();
            var toRemove = group.EmployeeGroups.Where(eg => !newEmployeeIds.Contains(eg.EmployeeId)).ToList();
            _context.EmployeeGroups.RemoveRange(toRemove);
            foreach (var empId in newEmployeeIds.Except(existingEmployeeIds))
            {
                _context.EmployeeGroups.Add(new EmployeeGroup
                {
                    GroupId = model.GroupId,
                    EmployeeId = empId
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

        /// <summary>
        /// Gets the groups.
        /// </summary>
        /// <returns>the view with the groups loaded</returns>
        [HttpGet]
        public async Task<IActionResult> GetGroups()
        {
            var groups = await _context.Groups
                .Select(g => new { g.Id, g.Name })
                .ToListAsync();
            return Json(groups);
        }

        /// <summary>
        /// Gets the employees.
        /// </summary>
        /// <returns>the view with employees loaded</returns>
        [HttpGet]
        public async Task<IActionResult> GetEmployees()
        {
            var employees = await _context.Users
                .Select(e => new { e.Id, e.UserName })
                .ToListAsync();
            return Json(employees);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveGroupAddition(int projectId, int groupId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return BadRequest(new { success = false, message = "User not authenticated" });
            }
            var projectGroup = await _context.ProjectGroups
                .Include(pg => pg.Group)
                .FirstOrDefaultAsync(pg => pg.ProjectId == projectId && pg.GroupId == groupId);
            if (projectGroup == null)
            {
                return NotFound(new { success = false, message = $"Group approval request not found for ProjectId {projectId}, GroupId {groupId}" });
            }
            if (projectGroup.Group.ManagerId != user.Id)
            {
                return ForbidJson(new { success = false, message = "Only the group manager can approve this request." });
            }
            projectGroup.IsApproved = true;
            await _context.SaveChangesAsync();
            return Json(new { success = true, message = "Group approved successfully." });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DenyGroupAddition(int projectId, int groupId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return BadRequest(new { success = false, message = "User not authenticated" });
            }

            var projectGroup = await _context.ProjectGroups
                .Include(pg => pg.Group)
                .FirstOrDefaultAsync(pg => pg.ProjectId == projectId && pg.GroupId == groupId && !pg.IsApproved);

            if (projectGroup == null)
            {
                return NotFound(new { success = false, message = $"Pending group request not found for ProjectId {projectId}, GroupId {groupId}" });
            }

            if (projectGroup.Group.ManagerId != user.Id)
            {
                return ForbidJson(new { success = false, message = "Only the group manager can deny this request." });
            }

            _context.ProjectGroups.Remove(projectGroup);
            await _context.SaveChangesAsync();

            return Json(new { success = true, message = "Group request denied successfully." });
        }


        private IActionResult ForbidJson(object value)
        {
            return new JsonResult(value) { StatusCode = StatusCodes.Status403Forbidden };
        }

    }
}
