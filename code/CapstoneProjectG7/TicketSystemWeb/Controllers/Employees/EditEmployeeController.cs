using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSystemWeb.Models.Employee;
using TicketSystemWeb.ViewModels;
using System.Threading.Tasks;

namespace TicketSystemWeb.Controllers
{
    public class EditEmployeeController : Controller
    {
        private readonly UserManager<Employee> _userManager;

        public EditEmployeeController(UserManager<Employee> userManager)
        {
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> EditEmployee(string id)
        {
            if (id == null) return BadRequest("Invalid employee ID.");

            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("Employee not found.");

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.Count > 0 ? roles[0] : "user";

            var viewModel = new EditEmployeeViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = role
            };

            return PartialView("EditEmployee", viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateEmployee(EditEmployeeViewModel model)
        {
            if (!ModelState.IsValid) return BadRequest("Invalid data.");

            var user = await _userManager.FindByIdAsync(model.Id);
            if (user == null) return NotFound("Employee not found.");

            user.UserName = model.UserName;
            user.Email = model.Email;

            var existingRoles = await _userManager.GetRolesAsync(user);
            if (existingRoles.Count > 0)
            {
                await _userManager.RemoveFromRolesAsync(user, existingRoles);
            }
            await _userManager.AddToRoleAsync(user, model.Role);

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("", "Error updating employee.");
                return PartialView("EditEmployee", model);
            }

            return RedirectToAction("Employees", "Employees");
        }
    }
}
