using Microsoft.AspNetCore.Mvc;
using TicketSystemWeb.Data;
using TicketSystemWeb.Models.KanbanBoard;
using Microsoft.EntityFrameworkCore;

namespace TicketSystemWeb.Controllers
{

    /// <summary>
    /// the home controller
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
        public async Task<IActionResult> Index()
        {
            var board = await _context.KanbanBoards
                .Include(b => b.Columns)
                    .ThenInclude(c => c.Tickets)
                .FirstOrDefaultAsync();

            if (board == null)
            {
                return View("NoBoardFound");
            }
            return View(board);
        }

        /// <summary>
        /// Adds the ticket.
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        /// <returns>the ticket was added</returns>
        [HttpPost]
        public async Task<IActionResult> AddTicket(Ticket ticket)
        {
            if (ModelState.IsValid)
            {
                ticket.CreatedAt = DateTime.UtcNow;
                ticket.CreatedBy = "System";
                ticket.Status = "Open";

                var firstColumn = await _context.KanbanColumns.OrderBy(c => c.Order).FirstOrDefaultAsync();
                if (firstColumn == null) return NotFound("No columns found.");

                ticket.ColumnId = firstColumn.Id;
                _context.Tickets.Add(ticket);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }

            var board = await _context.KanbanBoards
                .Include(b => b.Columns)
                    .ThenInclude(c => c.Tickets)
                .FirstOrDefaultAsync();
            return View("Index", board);
        }

        /// <summary>
        /// Moves the ticket.
        /// </summary>
        /// <param name="ticketId">The ticket identifier.</param>
        /// <param name="columnId">The column identifier.</param>
        /// <returns>the ticket was moved</returns>
        [HttpPost]
        public async Task<IActionResult> MoveTicket(int ticketId, int columnId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            var column = await _context.KanbanColumns.FindAsync(columnId);
            if (ticket == null || column == null) return NotFound();
            ticket.ColumnId = columnId;
            await _context.SaveChangesAsync();
            return new JsonResult(new { success = true });
        }

        /// <summary>
        /// Swaps the columns.
        /// </summary>
        /// <param name="draggedColumnId">The dragged column identifier.</param>
        /// <param name="targetColumnId">The target column identifier.</param>
        /// <returns>the view with the columns moved</returns>
        [HttpPost]
        public async Task<IActionResult> SwapColumns(int draggedColumnId, int targetColumnId)
        {
            var draggedColumn = await _context.KanbanColumns.FindAsync(draggedColumnId);
            var targetColumn = await _context.KanbanColumns.FindAsync(targetColumnId);
            if (draggedColumn == null || targetColumn == null) return NotFound();
            int tempOrder = draggedColumn.Order;
            draggedColumn.Order = targetColumn.Order;
            targetColumn.Order = tempOrder;
            await _context.SaveChangesAsync();
            return new JsonResult(new { success = true });
        }

    }

}
