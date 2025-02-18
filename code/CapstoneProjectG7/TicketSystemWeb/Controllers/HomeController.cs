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

        public IActionResult Index()
        {
            var tickets = _context.Tickets.ToList(); // Fetch all tickets
            return View(tickets);
        }
    }
}
