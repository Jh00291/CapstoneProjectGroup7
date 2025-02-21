using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSystemWeb.Data;
using TicketSystemWeb.Models.Employee;

namespace TicketSystemWeb.Controllers.Employees
{
    public class EmployeesController : Controller
    {
        private readonly ILogger<EmployeesController> _logger;
        private readonly TicketDBContext _context;
        private readonly UserManager<Employee> _userManager;

        public EmployeesController(
            ILogger<EmployeesController> logger,
            TicketDBContext context,
            UserManager<Employee> userManager)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> Employees()
        {
            var users = _context.Users.ToList();
            return View(users);
        }


    }
}
