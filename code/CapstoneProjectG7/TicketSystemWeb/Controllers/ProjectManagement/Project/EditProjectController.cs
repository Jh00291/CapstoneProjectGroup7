using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TicketSystemWeb.Data;
using TicketSystemWeb.Models.Employee;
using TicketSystemWeb.ViewModels;

namespace TicketSystemWeb.Controllers
{
    [Authorize]
    public class EditProjectController : Controller
    {
        private readonly TicketDBContext _context;
        private readonly UserManager<Employee> _userManager;

        public EditProjectController(TicketDBContext context, UserManager<Employee> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateProject(EditProjectViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid project data.");
            }

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var project = await _context.Projects.FindAsync(model.ProjectId);
            if (project == null) return NotFound();

            bool isAdmin = await _userManager.IsInRoleAsync(user, "admin");

            if (!isAdmin && project.ProjectManagerId != user.Id)
            {
                return Forbid();
            }

            project.Title = model.Title;
            project.Description = model.Description;

            _context.Projects.Update(project);
            await _context.SaveChangesAsync();

            return RedirectToAction("Management", "ProjectManagement");
        }
    }
}
