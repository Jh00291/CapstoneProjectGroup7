using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TicketSystemWeb.Data;
using TicketSystemWeb.Models.ProjectManagement.Group;
using TicketSystemWeb.Models.Employee;
using TicketSystemWeb.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace TicketSystemWeb.Controllers
{
    [Authorize]
    public class GroupManagementController : Controller
    {
        private readonly TicketDBContext _context;
        private readonly UserManager<Employee> _userManager;

        public GroupManagementController(TicketDBContext context, UserManager<Employee> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroup(AddGroupViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid group data.");
            }
            var manager = await _context.Users.FindAsync(model.SelectedManagerId);
            if (manager == null)
            {
                ModelState.AddModelError("SelectedManagerId", "Invalid Manager selected.");
                return View("Management", new ManagementViewModel
                {
                    Groups = await _context.Groups.ToListAsync(),
                    Projects = await _context.Projects.ToListAsync(),
                    AllEmployees = await _context.Users.ToListAsync()
                });
            }
            var group = new Group
            {
                Name = model.Name,
                ManagerId = model.SelectedManagerId,
                Manager = manager
            };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();
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
            return RedirectToAction("Management", "ProjectManagement");
        }

    }
}
