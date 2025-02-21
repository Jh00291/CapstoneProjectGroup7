using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSystemWeb.Data;
using TicketSystemWeb.Models;

namespace TicketSystemWeb.Controllers.Employees
{
    [Authorize(Roles = "admin")]
    public class EmployeesController : Controller
    {
        private readonly ILogger<EmployeesController> _logger;
        private readonly TicketDBContext _context;
        private readonly UserManager<User> _userManager;

        public EmployeesController(
            ILogger<EmployeesController> logger,
            TicketDBContext context,
            UserManager<User> userManager)
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
