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
        public async Task<IActionResult> RemoveEmployee(string UserId)
        {
            var user = await _userManager.FindByIdAsync(UserId);
            if (user != null)
            {
                var result = await _userManager.DeleteAsync(user);
                if (result.Succeeded)
                {
                    return RedirectToAction("Employees", "Employees");
                }
                else
                {
                    ModelState.AddModelError("", "Error deleting user.");
                }
            }
            return View();
        }
    }
}
