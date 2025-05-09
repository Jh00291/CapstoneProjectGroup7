﻿using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSystemWeb.Data;
using TicketSystemWeb.Models.Employee;
using TicketSystemWeb.ViewModels;

namespace TicketSystemWeb.Controllers
{
    /// <summary>
    /// the employees controller
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    public class EmployeesController : Controller
    {
        private readonly ILogger<EmployeesController> _logger;
        private readonly TicketDBContext _context;
        private readonly UserManager<Employee> _userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="EmployeesController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The context.</param>
        /// <param name="userManager">The user manager.</param>
        public EmployeesController(
            ILogger<EmployeesController> logger,
            TicketDBContext context,
            UserManager<Employee> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        /// <summary>
        /// Employeeses this instance.
        /// </summary>
        /// <returns>the view with the employees listed</returns>
        public async Task<IActionResult> Employees()
        {
            var employees = await _context.Users.ToListAsync();
            var loggedInUser = await _userManager.GetUserAsync(User);
            var loggedInUserId = loggedInUser?.Id;
            var loggedInUserRoles = await _userManager.GetRolesAsync(loggedInUser);
            bool isAdmin = loggedInUserRoles.Contains("admin");
            if (isAdmin)
            {
                ViewBag.CanManageEmployees = employees.Select(e => e.Id).ToList();
            }
            else
            {
                var managedEmployeeIds = await _context.EmployeeGroups
                    .Where(eg => eg.Group.ManagerId == loggedInUserId)
                    .Select(eg => eg.EmployeeId)
                    .ToListAsync();
                ViewBag.CanManageEmployees = managedEmployeeIds;
            }
            ViewBag.CurrentUserId = loggedInUserId;
            var viewModel = new AddEmployeeViewModel();
            return View(Tuple.Create(employees, viewModel));
        }

        /// <summary>
        /// Adds the employee.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>the employees page</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEmployee(AddEmployeeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var employees = await _context.Users.ToListAsync();
                return View("Employees", Tuple.Create(employees, model));
            }
            var existingUser = await _userManager.FindByNameAsync(model.UserName);
            if (existingUser != null)
            {
                var employees = await _context.Users.ToListAsync();
                return View("Employees", Tuple.Create(employees, model));
            }
            var user = new Employee
            {
                UserName = model.UserName,
                Email = model.Email,
                EmailConfirmed = true
            };
            var result = await _userManager.CreateAsync(user, model.Password);
            if (!result.Succeeded)
            {
                ViewData["ErrorMessage"] = string.Join(", ", result.Errors.Select(e => e.Description));
                var employees = await _context.Users.ToListAsync();
                return View("Employees", Tuple.Create(employees, model));
            }
            await _userManager.AddToRoleAsync(user, model.Role);
            return RedirectToAction(nameof(Employees));
        }

        /// <summary>
        /// Edits the employee.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>the employees page</returns>
        [HttpGet]
        public async Task<IActionResult> EditEmployee(string id)
        {
            if (string.IsNullOrEmpty(id)) return BadRequest("Invalid employee ID.");
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound("Employee not found.");
            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault() ?? "user";
            var viewModel = new EditEmployeeViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Role = role
            };
            return PartialView("EditEmployee", viewModel);
        }

        /// <summary>
        /// Updates the employee.
        /// </summary>
        /// <param name="model">The model.</param>
        /// <returns>the employees page</returns>
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
            if (existingRoles.FirstOrDefault() != model.Role)
            {
                await _userManager.RemoveFromRolesAsync(user, existingRoles);
                await _userManager.AddToRoleAsync(user, model.Role);
            }
            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
            {
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }
                return PartialView("EditEmployee", model);
            }
            return RedirectToAction(nameof(Employees));
        }

        /// <summary>
        /// Removes the employee.
        /// </summary>
        /// <param name="employeeId">The employee identifier.</param>
        /// <returns>the employees page</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RemoveEmployee(string employeeId)
        {
            if (string.IsNullOrEmpty(employeeId))
                return BadRequest("Invalid employee ID.");
            var user = await _userManager.FindByIdAsync(employeeId);
            if (user == null)
                return NotFound("Employee not found.");
            var managedGroups = await _context.Groups
                .Where(g => g.ManagerId == employeeId)
                .Select(g => g.Name)
                .ToListAsync();
            if (managedGroups.Any())
            {
                string groupNames = string.Join(", ", managedGroups);
                return BadRequest($"Please remove this user from {groupNames} before removing.");
            }
            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest("Failed to remove the employee.");
            }
            return Ok();
        }
    }
}
