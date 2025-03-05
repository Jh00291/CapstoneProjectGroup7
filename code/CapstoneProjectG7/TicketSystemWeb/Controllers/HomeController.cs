using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TicketSystemWeb.Data;
using TicketSystemWeb.Models.KanbanBoard;
using TicketSystemWeb.Models.ProjectManagement.Project;

namespace TicketSystemWeb.Controllers
{
    /// <summary>
    /// The home controller
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
        /// <param name="context">The database context.</param>
        public HomeController(ILogger<HomeController> logger, TicketDBContext context)
        {
            _logger = logger;
            _context = context;
        }

        /// <summary>
        /// Displays the Kanban board for a specific project.
        /// </summary>
        /// <param name="projectId">The project ID.</param>
        /// <returns>The project Kanban board view.</returns>
        public async Task<IActionResult> Index(int projectId)
        {
            var userId = GetLoggedInUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }
            List<Project> userProjects;
            if (User.IsInRole("admin"))
            {
                userProjects = await _context.Projects.ToListAsync();
            }
            else
            {
                userProjects = await _context.Projects
                    .Where(p => p.ProjectGroups
                        .Any(pg => pg.Group.EmployeeGroups.Any(eg => eg.EmployeeId == userId)))
                    .ToListAsync();
            }
            if (!userProjects.Any()) return View("NoProjectFound");
            if (projectId == 0) projectId = userProjects.First().Id;
            var project = await _context.Projects
                .Include(p => p.KanbanBoard)
                    .ThenInclude(b => b.Columns)
                .Include(p => p.Tickets)
                .FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null)
            {
                return View("NoProjectFound");
            }
            ViewBag.UserProjects = userProjects;
            if (project.KanbanBoard != null)
            {
                return View(project.KanbanBoard);
            }
            return View("Error"); // Error since the KanbanBoard is null
        }


        private string GetLoggedInUserId()
        {
            return User?.Identity?.IsAuthenticated == true ? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value : string.Empty;
        }

        /// <summary>
        /// Adds a new ticket to a project.
        /// </summary>
        /// <param name="projectId">The project ID.</param>
        /// <param name="ticket">The new ticket.</param>
        /// <returns>Redirects to the Kanban board.</returns>
        [HttpPost]
        public async Task<IActionResult> AddTicket(int projectId, Ticket ticket)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            var project = await _context.Projects
                .Include(p => p.KanbanBoard)
                .FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null) return NotFound("Project not found.");
            ticket.CreatedAt = DateTime.UtcNow;
            ticket.CreatedBy = "System";
            ticket.ProjectId = projectId;
            ticket.Status = "To Do";
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            return RedirectToAction("Index", new { projectId });
        }

        /// <summary>
        /// Moves a ticket between Kanban columns.
        /// </summary>
        /// <param name="ticketId">The ticket ID.</param>
        /// <param name="columnId">The target column ID.</param>
        /// <returns>Returns success if the operation is successful.</returns>
        [HttpPost]
        public async Task<IActionResult> MoveTicket(int ticketId, int columnId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            var column = await _context.KanbanColumns.FindAsync(columnId);
            if (ticket == null || column == null) return NotFound();
            ticket.Status = column.Name;
            await _context.SaveChangesAsync();
            return new JsonResult(new { success = true });
        }

        /// <summary>
        /// Swaps the order of two columns.
        /// </summary>
        /// <param name="draggedColumnId">The dragged column ID.</param>
        /// <param name="targetColumnId">The target column ID.</param>
        /// <returns>Returns success if the swap was successful.</returns>
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
