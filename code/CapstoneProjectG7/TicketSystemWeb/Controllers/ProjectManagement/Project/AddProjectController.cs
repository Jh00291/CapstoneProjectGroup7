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
    public class AddProjectController : Controller
    {
        private readonly TicketDBContext _context;
        private readonly UserManager<Employee> _userManager;

        public AddProjectController(TicketDBContext context, UserManager<Employee> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AddProjectViewModel model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            bool isAdmin = User.IsInRole("Admin");
            bool isGroupManager = await _context.Groups.AnyAsync(g => g.ManagerId == user.Id);

            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid project details.");
            }
            var project = new Models.ProjectManagement.Project.Project
            {
                Title = model.Title,
                Description = model.Description,
                ProjectManagerId = user.Id,
                ProjectManager = user
            };

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return RedirectToAction("Management", "ProjectManagement");
        }
    }
}
