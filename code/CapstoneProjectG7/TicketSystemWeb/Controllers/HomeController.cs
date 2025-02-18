using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using TicketSystemWeb.Data;
using TicketSystemWeb.Models;
using System.Linq;

namespace TicketSystemWeb.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TicketDBContext _context;

        public HomeController(ILogger<HomeController> logger, TicketDBContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var tickets = _context.Tickets.ToList(); // Fetch all tickets
            return View(tickets);
        }

        [HttpPost]
        public IActionResult AddTicket(Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.CreatedAt = DateTime.UtcNow; // Set creation date
                _context.Tickets.Add(ticket);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            var tickets = _context.Tickets.ToList();
            return View("Index", tickets);
        }
    }
}
