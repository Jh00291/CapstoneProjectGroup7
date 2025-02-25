using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSystemWeb.Data;
using TicketSystemWeb.Models.Employee;
using TicketSystemWeb.Models.ProjectManagement.Group;
using TicketSystemWeb.ViewModels;

namespace TicketSystemWeb.Controllers
{
    [Authorize]
    public class AddGroupController : Controller
    {
        private readonly TicketDBContext _context;
        private readonly UserManager<Employee> _userManager;

        public AddGroupController(TicketDBContext context, UserManager<Employee> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> AddGroup()
        {
            var employees = await _context.Users.ToListAsync();
            var viewModel = new AddGroupViewModel
            {
                AllEmployees = employees
            };
            return PartialView("AddGroup", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateGroup(AddGroupViewModel model)
        {
            var newGroup = new Group
            {
                Name = model.Name,
                ManagerId = model.SelectedManagerId
            };
            _context.Groups.Add(newGroup);
            await _context.SaveChangesAsync();
            if (model.SelectedEmployeeIds != null && model.SelectedEmployeeIds.Any())
            {
                foreach (var employeeId in model.SelectedEmployeeIds)
                {
                    _context.EmployeeGroups.Add(new EmployeeGroup
                    {
                        GroupId = newGroup.Id,
                        EmployeeId = employeeId
                    });
                }
                await _context.SaveChangesAsync();
            }
            return RedirectToAction("Management", "ProjectManagement");
        }
    }
}
