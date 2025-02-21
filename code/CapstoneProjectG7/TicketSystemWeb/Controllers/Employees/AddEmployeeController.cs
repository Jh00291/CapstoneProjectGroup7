using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TicketSystemWeb.Models;

namespace TicketSystemWeb.Controllers
{
    public class AddEmployeeController : Controller
    {
        private readonly UserManager<User> _userManager;

        public AddEmployeeController(UserManager<User> userManager)
        {
            _userManager = userManager;
        }

        [HttpPost]
        public async Task<IActionResult> AddEmployee(string UserName, string Email, string Password, string Role)
        {
            if (ModelState.IsValid)
            {
                var user = new User { UserName = UserName, Email = Email, EmailConfirmed = true };
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
