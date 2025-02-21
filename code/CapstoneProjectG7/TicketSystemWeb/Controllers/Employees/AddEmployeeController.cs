using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TicketSystemWeb.Models.Employee;

namespace TicketSystemWeb.Controllers
{
    public class AddEmployeeController : Controller
    {
        private readonly UserManager<Employee> _userManager;

        public AddEmployeeController(UserManager<Employee> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> AddEmployee(string UserName, string Email, string Password, string Role)
        {
            if (ModelState.IsValid)
            {
                var user = new Employee { UserName = UserName, Email = Email, EmailConfirmed = true };
                var result = await _userManager.CreateAsync(user, Password);

                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, Role);
                    return RedirectToAction("Employees", "Employees");
                }
                else
                {
                    ModelState.AddModelError("", "Error creating user.");
                }
            }
            return View();
        }
    }
}
