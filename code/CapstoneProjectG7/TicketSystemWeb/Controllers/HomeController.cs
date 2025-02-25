using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using TicketSystemWeb.Data;
using TicketSystemWeb.Models;
using System.Linq;

namespace TicketSystemWeb.Controllers
{

    /// <summary>
    /// the home page controller
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Mvc.Controller" />
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TicketDBContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="HomeController"/> class.
        /// </summary>
        /// <param name="logger">The logger.</param>
        /// <param name="context">The context.</param>
        public HomeController(ILogger<HomeController> logger, TicketDBContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Indexes this instance.
        /// </summary>
        /// <returns>the home page</returns>
        [HttpGet]
        public IActionResult Index()
        {
            var tickets = _context.Tickets.ToList();
            return View(tickets);
        }

        /// <summary>
        /// Adds the ticket.
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        /// <returns>the home page</returns>
        [HttpPost]
        public IActionResult AddTicket(Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.CreatedAt = DateTime.UtcNow;
                _context.Tickets.Add(ticket);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            var tickets = _context.Tickets.ToList();
            return View("Index", tickets);
        }
    }
}
