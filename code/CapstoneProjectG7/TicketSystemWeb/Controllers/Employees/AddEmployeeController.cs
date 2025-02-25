using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TicketSystemWeb.Models.Employee;
using TicketSystemWeb.ViewModels;

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
        public async Task<IActionResult> AddEmployee(AddEmployeeViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new Employee { UserName = model.UserName, Email = model.Email, EmailConfirmed = true };
                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, model.Role);
                    return RedirectToAction("Employees", "Employees");
                }
                else
                {
                    ModelState.AddModelError("", "Error creating user.");
                }
            }
            return PartialView("AddEmployee", model);
        }
    }
}
