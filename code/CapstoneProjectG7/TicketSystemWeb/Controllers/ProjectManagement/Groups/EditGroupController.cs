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
    public class EditGroupController : Controller
    {
        private readonly TicketDBContext _context;
        private readonly UserManager<Employee> _userManager;

        public EditGroupController(TicketDBContext context, UserManager<Employee> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateGroup(EditGroupViewModel model)
        {
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

            _context.EmployeeGroups.RemoveRange(_context.EmployeeGroups.Where(eg => eg.GroupId == model.GroupId));
            foreach (var employeeId in model.SelectedEmployeeIds)
            {
                _context.EmployeeGroups.Add(new EmployeeGroup { GroupId = model.GroupId, EmployeeId = employeeId });
            }
            await _context.SaveChangesAsync();
            return RedirectToAction("Management", "ProjectManagement");
        }
    }
}
