using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TicketSystemWeb.Models.Employee;

namespace TicketSystemWeb.Controllers
{
    public class RemoveEmployeeController : Controller
    {
        private readonly UserManager<Employee> _userManager;

        public RemoveEmployeeController(UserManager<Employee> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveEmployee(string employeeId)
        {
            if (employeeId == null) return BadRequest("Invalid employee ID.");

            var user = await _userManager.FindByIdAsync(employeeId);
            if (user == null) return NotFound("Employee not found.");

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Error removing employee.");
                return RedirectToAction("Employees", "Employees");
            }

            return RedirectToAction("Employees", "Employees");
        }
    }
}
