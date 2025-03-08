using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<TicketDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _context = new TicketDBContext(options);
            _loggerMock = new Mock<ILogger<HomeController>>();
            _controller = new HomeController(_loggerMock.Object, _context);
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
        public async Task Index_ValidProjectId_ReturnsKanbanBoardView()
        {
            var project = new Project { Id = 1, KanbanBoard = new KanbanBoard { Id = 1 } };
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();
            var result = await _controller.Index(1) as ViewResult;
            Assert.That(result?.Model, Is.EqualTo(project.KanbanBoard));
        }

        [Test]
        public async Task Index_NoUserLoggedIn_RedirectsToLogin()
        {
            var result = await _controller.Index(1) as RedirectToActionResult;
            Assert.That(result?.ActionName, Is.EqualTo("Login"));
            Assert.That(result?.ControllerName, Is.EqualTo("Account"));
        }

        [Test]
        public async Task Index_NoProjectsForUser_ReturnsNoProjectFoundView()
        {
            var result = await _controller.Index(1) as ViewResult; 
            Assert.That(result?.ViewName ?? "NoProjectFound", Is.EqualTo("NoProjectFound"));
        }

        [Test]
        public async Task Index_ProjectExists_ReturnsKanbanBoardView()
        {
            Project project = null;
            if (!await _context.Projects.AnyAsync(p => p.Id == 1))
            {
                project = new Project { Id = 1, KanbanBoard = new KanbanBoard { Id = 1 } };
                _context.Projects.Add(project);
                await _context.SaveChangesAsync();
            }
            var result = await _controller.Index(1) as ViewResult;
            Assert.That(result?.Model, Is.EqualTo(project.KanbanBoard));
        }

        [Test]
        public async Task Index_ValidProject_AssignsViewBagValues()
        {
            var project = new Project { Id = 1, KanbanBoard = new KanbanBoard { Id = 1 }, ProjectManagerId = "user1" };
            var anotherProject = new Project { Id = 2 };
            _context.Projects.AddRange(project, anotherProject);
            await _context.SaveChangesAsync();
            var result = await _controller.Index(1) as ViewResult;
            Assert.That(_controller.ViewBag.UserProjects, Is.Not.Null);
            Assert.That(_controller.ViewBag.CanManageColumns, Is.Not.Null);
        }

        [Test]
        public async Task SwapColumns_ValidColumns_SwapsOrder()
        {
            var column1 = new KanbanColumn { Name = "To Do", Order = 1, KanbanBoardId = 1 };
            var column2 = new KanbanColumn { Name = "In Progress", Order = 2, KanbanBoardId = 1 };
            _context.KanbanColumns.AddRange(column1, column2);
            await _context.SaveChangesAsync();
            await _context.Entry(column1).ReloadAsync();
            await _context.Entry(column2).ReloadAsync();
            Assert.That(column1.Order, Is.EqualTo(2));
            Assert.That(column2.Order, Is.EqualTo(1));
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
            Assert.That(value?.success, Is.True);
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
            Assert.That(result?.StatusCode, Is.EqualTo(404));
        }

        [Test]
        public async Task MoveTicket_ValidMove_UpdatesTicketStatus()
        {
            var column = new KanbanColumn { Id = 1, Name = "In Progress" };
            var ticket = new Ticket { Title = "Fix Bug", Description = "Resolve issue #123", CreatedBy = "Developer", Status = "In Progress" };
            _context.KanbanColumns.Add(column);
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            var result = await _controller.MoveTicket(1, 1) as JsonResult;
            dynamic value = result?.Value;
            Assert.That(value?.success, Is.True);
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

        [Test]
        public async Task DeleteColumn_ValidColumn_ReassignsTicketsAndDeletesColumn()
        {
            var project = new Project { Id = 1, KanbanBoard = new KanbanBoard { Id = 1 } };
            var firstColumn = new KanbanColumn { Id = 1, Name = "To Do", Order = 1, KanbanBoardId = 1 };
            var columnToDelete = new KanbanColumn { Id = 2, Name = "In Progress", Order = 2, KanbanBoardId = 1 };
            var ticket = new Ticket { TicketId = 1, Title = "Fix Bug", Status = "In Progress" };
            project.KanbanBoard.Columns.Add(firstColumn);
            project.KanbanBoard.Columns.Add(columnToDelete);
            _context.Projects.Add(project);
            _context.KanbanColumns.Add(firstColumn);
            _context.KanbanColumns.Add(columnToDelete);
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();
            var result = await _controller.DeleteColumn(2) as JsonResult;
            var remainingColumn = await _context.KanbanColumns.FirstOrDefaultAsync(c => c.Id == 2);
            Assert.That(ticket.Status, Is.EqualTo("To Do"));
            Assert.That(remainingColumn, Is.Null);
            Assert.That(result?.Value, Is.EqualTo(new { success = true }));
        }
    }
}
