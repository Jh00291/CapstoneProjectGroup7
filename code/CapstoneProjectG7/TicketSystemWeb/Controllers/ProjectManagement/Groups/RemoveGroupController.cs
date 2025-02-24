using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSystemWeb.Data;
using TicketSystemWeb.Models.Employee;

namespace TicketSystemWeb.Controllers
{
    [Authorize]
    public class RemoveGroupController : Controller
    {
        private readonly TicketDBContext _context;
        private readonly UserManager<Employee> _userManager;

        public RemoveGroupController(TicketDBContext context, UserManager<Employee> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteGroup(int groupId)
        {
            var group = await _context.Groups
                .Include(g => g.EmployeeGroups)
                .FirstOrDefaultAsync(g => g.Id == groupId);

            if (group == null) return NotFound();

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

            return RedirectToAction("Management", "ProjectManagement");
        }
    }
}
