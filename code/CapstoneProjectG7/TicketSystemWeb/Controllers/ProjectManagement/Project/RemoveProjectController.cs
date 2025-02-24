using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TicketSystemWeb.Data;
using TicketSystemWeb.Models.Employee;

namespace TicketSystemWeb.Controllers
{
    [Authorize]
    public class RemoveProjectController : Controller
    {
        private readonly TicketDBContext _context;
        private readonly UserManager<Employee> _userManager;

        public RemoveProjectController(TicketDBContext context, UserManager<Employee> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProject(int projectId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var project = await _context.Projects.FindAsync(projectId);
            if (project == null) return NotFound();

            bool isAdmin = await _userManager.IsInRoleAsync(user, "admin");

            if (!isAdmin || project.ProjectManagerId != user.Id)
            {
                return Forbid();
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return RedirectToAction("Management", "ProjectManagement");
        }
    }
}
