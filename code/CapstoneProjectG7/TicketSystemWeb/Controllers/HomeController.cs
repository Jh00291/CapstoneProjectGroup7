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
                    .Where(p =>
                        p.ProjectManagerId == userId ||
                        p.ProjectGroups.Any(pg => pg.Group.EmployeeGroups.Any(eg => eg.EmployeeId == userId))
                    )
                    .ToListAsync();
            }
            if (!userProjects.Any())
            {
                ViewBag.UserProjects = new List<Project>();
                ViewBag.CanManageColumns = false;
                return View(new KanbanBoard { Columns = new List<KanbanColumn>() });
            }
            if (projectId == 0) projectId = userProjects.First().Id;
            var project = await _context.Projects
                .Include(p => p.KanbanBoard)
                    .ThenInclude(b => b.Columns)
                .Include(p => p.Tickets)
                .FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null)
            {
                return View(new KanbanBoard { Columns = new List<KanbanColumn>() });
            }
            ViewBag.UserProjects = userProjects;
            ViewBag.CanManageColumns = User.IsInRole("admin") || project.ProjectManagerId == userId;
            ViewBag.CanMoveTickets = User.IsInRole("admin") || User.IsInRole("user") || project.ProjectManagerId == userId;
            return View(project.KanbanBoard);
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
        public async Task<IActionResult> AddTicket([FromBody] Ticket ticket)
        {
            if (ticket == null || string.IsNullOrWhiteSpace(ticket.Title) || string.IsNullOrWhiteSpace(ticket.Description))
                return BadRequest("Invalid ticket data.");

            var project = await _context.Projects
                .Include(p => p.KanbanBoard)
                    .ThenInclude(b => b.Columns)
                .FirstOrDefaultAsync(p => p.Id == ticket.ProjectId);

            if (project == null) return NotFound("Project not found.");

            var firstColumn = project.KanbanBoard.Columns
                .OrderBy(c => c.Order)
                .FirstOrDefault();

            if (firstColumn == null)
                return BadRequest("No columns available in the Kanban board.");
            var history = new TicketHistory
            {
                Ticket = ticket,
                Action = $"Ticket created in column '{firstColumn.Name}'",
                PerformedBy = User.Identity.Name ?? "System"
            };
            ticket.History.Add(history);
            ticket.CreatedAt = DateTime.UtcNow;
            ticket.CreatedBy = User.Identity.Name ?? "System";
            ticket.Status = firstColumn.Name;
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            return Ok(new { success = true });
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

            var kanbanBoard = await _context.KanbanBoards
                .Include(b => b.Columns)
                .FirstOrDefaultAsync(b => b.Id == column.KanbanBoardId);

            var firstColumn = kanbanBoard?.Columns.OrderBy(c => c.Order).FirstOrDefault();
            var userId = GetLoggedInUserId();

            if (ticket.Status == firstColumn?.Name && string.IsNullOrEmpty(ticket.AssignedToId))
            {
                ticket.AssignedToId = userId;
            }

            if (column.Name == firstColumn?.Name && ticket.AssignedToId != null)
            {
                ticket.AssignedToId = null;
            }

            ticket.Status = column.Name;
            var history = new TicketHistory
            {
                TicketId = ticketId,
                Action = $"Moved to column '{column.Name}'",
                PerformedBy = User.Identity.Name ?? "System"
            };
            _context.TicketHistories.Add(history);
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
            var kanbanBoard = await _context.KanbanColumns
                .Where(c => c.Id == draggedColumnId || c.Id == targetColumnId)
                .Select(c => c.KanbanBoardId)
                .FirstOrDefaultAsync();
            if (kanbanBoard == 0) return NotFound("Kanban board not found.");
            var columns = await _context.KanbanColumns
                .Where(c => c.KanbanBoardId == kanbanBoard)
                .OrderBy(c => c.Order)
                .ToListAsync();
            var draggedColumn = columns.FirstOrDefault(c => c.Id == draggedColumnId);
            var targetColumn = columns.FirstOrDefault(c => c.Id == targetColumnId);
            if (draggedColumn == null || targetColumn == null) return NotFound();
            int draggedOrder = draggedColumn.Order;
            draggedColumn.Order = targetColumn.Order;
            targetColumn.Order = draggedOrder;
            columns = columns.OrderBy(c => c.Order).ToList();
            for (int i = 0; i < columns.Count; i++)
            {
                columns[i].Order = i + 1;
            }
            await _context.SaveChangesAsync();
            return new JsonResult(new { success = true });
        }

        /// <summary>
        /// Renames the column.
        /// </summary>
        /// <param name="columnId">The column identifier.</param>
        /// <param name="newName">The new name.</param>
        /// <returns>the view with the renamed column</returns>
        [HttpPost]
        public async Task<IActionResult> RenameColumn(int columnId, string newName)
        {
            var column = await _context.KanbanColumns.FindAsync(columnId);
            if (column == null) return NotFound();
            column.Name = newName;
            await _context.SaveChangesAsync();
            return new JsonResult(new { success = true });
        }

        /// <summary>
        /// Adds the column.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <param name="name">The name.</param>
        /// <returns>the view with the added column</returns>
        [HttpPost]
        public async Task<IActionResult> AddColumn(int projectId, string name)
        {
            var project = await _context.Projects
                .Include(p => p.KanbanBoard)
                .FirstOrDefaultAsync(p => p.Id == projectId);
            if (project == null || project.KanbanBoard == null) return NotFound();
            int newOrder = await _context.KanbanColumns
                .Where(c => c.KanbanBoardId == project.KanbanBoard.Id)
                .MaxAsync(c => (int?)c.Order) ?? 0;
            var newColumn = new KanbanColumn
            {
                Name = name,
                Order = newOrder + 1,
                KanbanBoardId = project.KanbanBoard.Id
            };
            _context.KanbanColumns.Add(newColumn);
            await _context.SaveChangesAsync();
            return new JsonResult(new { success = true });
        }

        /// <summary>
        /// Deletes the column.
        /// </summary>
        /// <param name="columnId">The column identifier.</param>
        /// <returns>the view with the deleted column</returns>
        [HttpPost]
        public async Task<IActionResult> DeleteColumn(int columnId)
        {
            var column = await _context.KanbanColumns
                .Include(c => c.KanbanBoard)
                .FirstOrDefaultAsync(c => c.Id == columnId);
            if (column == null) return NotFound();
            var kanbanBoard = column.KanbanBoard;
            if (kanbanBoard == null) return BadRequest("Invalid Kanban Board.");
            var firstColumn = await _context.KanbanColumns
                .Where(c => c.KanbanBoardId == kanbanBoard.Id && c.Id != columnId)
                .OrderBy(c => c.Order)
                .FirstOrDefaultAsync();
            if (firstColumn == null) return BadRequest("No valid first column found.");
            var tickets = await _context.Tickets
                .Where(t => t.Status == column.Name)
                .ToListAsync();
            foreach (var ticket in tickets)
            {
                ticket.Status = firstColumn.Name;
            }
            await _context.SaveChangesAsync();
            _context.KanbanColumns.Remove(column);
            await _context.SaveChangesAsync();
            var remainingColumns = await _context.KanbanColumns
                .Where(c => c.KanbanBoardId == kanbanBoard.Id)
                .OrderBy(c => c.Order)
                .ToListAsync();
            for (int i = 0; i < remainingColumns.Count; i++)
            {
                remainingColumns[i].Order = i + 1;
            }
            await _context.SaveChangesAsync();
            return new JsonResult(new { success = true });
        }

        /// <summary>
        /// Retrieves details of a specific ticket.
        /// </summary>
        /// <param name="ticketId">The ticket ID.</param>
        /// <returns>JSON response with ticket details.</returns>
        [HttpGet]
        public async Task<IActionResult> ViewTicketDetails(int ticketId)
        {
            var ticket = await _context.Tickets
                .Include(t => t.AssignedTo)
                .Include(t => t.History.OrderByDescending(h => h.Timestamp))
                .Include(t => t.Comments.OrderBy(c => c.CreatedAt))
                .FirstOrDefaultAsync(t => t.TicketId == ticketId);
            if (ticket == null) return NotFound();
            return Json(new
            {
                success = true,
                ticket = new
                {
                    id = ticket.TicketId,
                    title = ticket.Title,
                    description = ticket.Description,
                    status = ticket.Status,
                    createdAt = ticket.CreatedAt.ToString("yyyy-MM-dd HH:mm"),
                    assignedTo = ticket.AssignedTo?.UserName,
                    assignedToId = ticket.AssignedToId,
                    history = ticket.History.Select(h => new
                    {
                        action = h.Action,
                        timestamp = h.Timestamp.ToString("yyyy-MM-dd HH:mm"),
                        performedBy = h.PerformedBy
                    }),
                    comments = ticket.Comments.Select(c => new
                    {
                        author = c.AuthorName,
                        text = c.CommentText
                    })
                }
            });
        }

        /// <summary>
        /// Edits a ticket.
        /// </summary>
        /// <param name="ticketId">The ticket ID.</param>
        /// <param name="title">The new title.</param>
        /// <param name="description">The new description.</param>
        /// <returns>Success response if edited.</returns>
        [HttpPost]
        public async Task<IActionResult> EditTicket(int ticketId, string title, string description, string? assignedToId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return NotFound();

            ticket.Title = title;
            ticket.Description = description;
            ticket.AssignedToId = string.IsNullOrWhiteSpace(assignedToId) ? null : assignedToId;
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        /// <summary>
        /// Deletes a ticket.
        /// </summary>
        /// <param name="ticketId">The ticket ID.</param>
        /// <returns>Success response if deleted.</returns>
        [HttpPost]
        public async Task<IActionResult> DeleteTicket(int ticketId)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return NotFound();

            _context.Tickets.Remove(ticket);
            await _context.SaveChangesAsync();

            return Json(new { success = true });
        }

        /// <summary>
        /// Adds the ticket comment.
        /// </summary>
        /// <param name="ticketId">The ticket identifier.</param>
        /// <param name="commentText">The comment text.</param>
        /// <returns>adds a ticket comments</returns>
        [HttpPost]
        public async Task<IActionResult> AddTicketComment(int ticketId, string commentText)
        {
            var ticket = await _context.Tickets.FindAsync(ticketId);
            if (ticket == null) return NotFound();
            var comment = new TicketComment
            {
                TicketId = ticketId,
                CommentText = commentText,
                AuthorName = User.Identity.Name ?? "System"
            };
            _context.TicketComments.Add(comment);
            await _context.SaveChangesAsync();
            return Json(new { success = true });
        }

        /// <summary>
        /// Gets the employees associated with a particular project.
        /// </summary>
        /// <param name="projectId">The project identifier.</param>
        /// <returns>List of employees on a project</returns>
        [HttpGet]
        public async Task<IActionResult> GetProjectEmployees(int projectId)
        {
            var employees = await _context.EmployeeGroups
                .Where(eg => eg.Group.ProjectGroups.Any(pg => pg.ProjectId == projectId))
                .Select(eg => new {
                    id = eg.Employee.Id,
                    name = eg.Employee.UserName
                })
                .Distinct()
                .ToListAsync();

            return Json(employees);
        }

    }
}
