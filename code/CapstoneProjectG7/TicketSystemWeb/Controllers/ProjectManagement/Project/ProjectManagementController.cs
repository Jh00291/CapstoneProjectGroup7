using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSystemWeb.Data;
using TicketSystemWeb.Models.Employee;
using TicketSystemWeb.ViewModels;

namespace TicketSystemWeb.Controllers
{
    [Authorize]
    public class ProjectManagementController : Controller
    {
        private readonly TicketDBContext _context;
        private readonly UserManager<Employee> _userManager;

        public ProjectManagementController(TicketDBContext context, UserManager<Employee> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

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

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> DeleteProject(int projectId)
        {
            var user = await _userManager.GetUserAsync(User);
            var project = await _context.Projects.FindAsync(projectId);

            if (project == null) return NotFound();
            if (project.ProjectManagerId != user.Id) return Forbid();

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return RedirectToAction("Management");
        }
    }
}
