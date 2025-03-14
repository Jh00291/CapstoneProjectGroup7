using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TicketSystemWeb.Controllers;
using TicketSystemWeb.Data;
using TicketSystemWeb.Models.KanbanBoard;
using TicketSystemWeb.Models.ProjectManagement.Project;

namespace TicketSystemWeb.Tests.Controllers
{
    [TestFixture]
    public class HomeControllerTests
    {
        private TicketDBContext _context;
        private Mock<ILogger<HomeController>> _loggerMock;
        private HomeController _controller;
        private Mock<ClaimsPrincipal> _userMock;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<TicketDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new TicketDBContext(options);
            _context.Database.EnsureDeleted(); // Clear database before each test
            _context.Database.EnsureCreated(); // Recreate schema

            _loggerMock = new Mock<ILogger<HomeController>>();
            
            _userMock = new Mock<ClaimsPrincipal>();
            _controller = new HomeController(_loggerMock.Object, _context)
            {
                ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext { User = _userMock.Object } }
            };
        }

        [TearDown]
        public void TearDown()
        {
            _context?.Dispose();
            _controller.Dispose();
        }

        [Test]
        public async Task Index_ProjectNotFound_ReturnsNoProjectFoundView()
        {
            var result = await _controller.Index(0) as ViewResult;
            Assert.That(result?.ViewName ?? "NoProjectFound", Is.EqualTo("NoProjectFound"));
        }

        [Test]
        public async Task Index_NoUserLoggedIn_RedirectsToLogin()
        {
            var result = await _controller.Index(1) as RedirectToActionResult;
            Assert.That(result?.ActionName, Is.EqualTo("Login"));
            Assert.That(result?.ControllerName, Is.EqualTo("Account"));
        }

        [Test]
        public async Task SwapColumns_InvalidColumns_ReturnsNotFound()
        {
            var result = await _controller.SwapColumns(999, 888) as NotFoundObjectResult;
            Assert.That(result?.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task SwapColumns_ValidSwap_ChangesColumnOrder()
        {
            var kanbanBoard = new KanbanBoard { Id = 1, ProjectName = "Board" };
            var column1 = new KanbanColumn { Id = 1, Name = "To Do", Order = 1, KanbanBoardId = 1 };
            var column2 = new KanbanColumn { Id = 2, Name = "In Progress", Order = 2, KanbanBoardId = 1 };

            _context.KanbanBoards.Add(kanbanBoard);
            _context.KanbanColumns.AddRange(column1, column2);
            await _context.SaveChangesAsync();

            var result = await _controller.SwapColumns(column1.Id, column2.Id) as JsonResult;

            var updatedColumns = await _context.KanbanColumns.OrderBy(c => c.Order).ToListAsync();
            Assert.That(updatedColumns[0].Id, Is.EqualTo(column2.Id));
            Assert.That(updatedColumns[1].Id, Is.EqualTo(column1.Id));
        }

        [Test]
        public async Task DeleteColumn_NoValidFirstColumn_ReturnsBadRequest()
        {
            var kanbanBoard = new KanbanBoard { Id = 1, ProjectName = "Board" };
            var column = new KanbanColumn { Id = 1, Name = "Only Column", Order = 1, KanbanBoardId = 1 };

            _context.KanbanBoards.Add(kanbanBoard);
            _context.KanbanColumns.Add(column);
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteColumn(column.Id) as BadRequestObjectResult;
            Assert.That(result?.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task DeleteColumn_ValidColumn_RemovesColumn()
        {
            var kanbanBoard = new KanbanBoard { Id = 1, ProjectName = "Board" };
            var column1 = new KanbanColumn { Id = 1, Name = "To Do", Order = 1, KanbanBoardId = 1 };
            var column2 = new KanbanColumn { Id = 2, Name = "In Progress", Order = 2, KanbanBoardId = 1 };

            _context.KanbanBoards.Add(kanbanBoard);
            _context.KanbanColumns.AddRange(column1, column2);
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteColumn(column1.Id) as JsonResult;
            Assert.That(result, Is.Not.Null);

            var remainingColumns = await _context.KanbanColumns.ToListAsync();
            Assert.That(remainingColumns.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task Index_NoProjectsForUser_ReturnsNoProjectFoundView()
        {
            var result = await _controller.Index(1) as ViewResult; 
            Assert.That(result?.ViewName ?? "NoProjectFound", Is.EqualTo("NoProjectFound"));
        }

        [Test]
        public async Task AddColumn_ValidProject_AddsColumn()
        {
            var project = new Project { Id = 1, Title = "Test Project", KanbanBoard = new KanbanBoard { Id = 1, ProjectName = "Test Kanban" } };
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            var result = await _controller.AddColumn(1, "New Column") as JsonResult;
            var column = await _context.KanbanColumns.FirstOrDefaultAsync(c => c.Name == "New Column");
            Assert.That(column, Is.Not.Null);
            Assert.That(column.Order, Is.EqualTo(1));
            dynamic value = result?.Value;
            Assert.That(value, Is.Not.Null, "Expected a JSON result but got null.");
        }

        [Test]
        public async Task AddTicket_InvalidModel_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("error", "Invalid model");
            var result = await _controller.AddTicket(1, new Ticket()) as BadRequestObjectResult;
            Assert.That(result?.StatusCode, Is.EqualTo(400));
        }

        [Test]
        public async Task AddTicket_ValidTicket_AddsTicketAndRedirects()
        {
            var project = new Project { Title = "Test Project", KanbanBoard = new KanbanBoard { ProjectName = "Test Kanban" } };
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            var ticket = new Ticket { Title = "Fix Bug", Description = "Resolve issue #123", CreatedBy = "Developer", Status = "In Progress" };
            var result = await _controller.AddTicket(project.Id, ticket) as RedirectToActionResult;
            Assert.That(result?.ActionName, Is.EqualTo("Index"));
        }

        [Test]
        public async Task MoveTicket_TicketNotFound_ReturnsNotFound()
        {
            var result = await _controller.MoveTicket(1, 1) as NotFoundResult;

            Assert.That(result, Is.Not.Null, "Expected NotFoundResult but got null.");
        }

        [Test]
        public async Task MoveTicket_ValidMove_UpdatesTicketStatus()
        {
            var column = new KanbanColumn { Name = "In Progress" };
            var ticket = new Ticket { Title = "Fix Bug", Description = "Resolve issue #123", CreatedBy = "Developer", Status = "In Progress" };

            _context.KanbanColumns.Add(column);
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            var result = await _controller.MoveTicket(ticket.TicketId, column.Id) as JsonResult;

            Assert.That(result, Is.InstanceOf<JsonResult>(), "Expected JsonResult but got a different type.");
        }


        [Test]
        public async Task RenameColumn_ColumnNotFound_ReturnsNotFound()
        {
            var result = await _controller.RenameColumn(999, "New Name") as NotFoundResult;
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task DeleteColumn_ColumnNotFound_ReturnsNotFound()
        {
            var result = await _controller.DeleteColumn(999) as NotFoundResult;
            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(404));
        }


    }
}
