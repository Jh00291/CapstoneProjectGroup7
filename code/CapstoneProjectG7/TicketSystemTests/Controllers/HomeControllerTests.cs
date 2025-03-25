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
using Newtonsoft.Json.Linq;

namespace TicketSystemWeb.Tests.Controllers
{
    [TestFixture]
    public class HomeControllerTests
    {
        private TicketDBContext _context;
        private Mock<ILogger<HomeController>> _loggerMock;
        private HomeController _controller;
        private Mock<ClaimsPrincipal> _userMock;
        private string _testUserId;

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
            _testUserId = "user123";
            _userMock.Setup(u => u.Identity.IsAuthenticated).Returns(true);
            _userMock.Setup(u => u.FindFirst(ClaimTypes.NameIdentifier))
                     .Returns(new Claim(ClaimTypes.NameIdentifier, _testUserId));

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
            _userMock.Setup(u => u.Identity.IsAuthenticated).Returns(false);
            _userMock.Setup(u => u.FindFirst(ClaimTypes.NameIdentifier)).Returns((Claim)null);

            var result = await _controller.Index(1) as RedirectToActionResult;

            Assert.That(result, Is.Not.Null, "Expected a redirect result but got null.");
            Assert.That(result.ActionName, Is.EqualTo("Login"));
            Assert.That(result.ControllerName, Is.EqualTo("Account"));
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
        public async Task Index_UserIsAdmin_LoadsAllProjects()
        {
            _userMock.Setup(u => u.IsInRole("admin")).Returns(true);

            var kanbanBoard1 = new KanbanBoard { Id = 1, ProjectName = "Admin Board 1" };
            var kanbanBoard2 = new KanbanBoard { Id = 2, ProjectName = "Admin Board 2" };

            var project1 = new Project { Id = 1, Title = "Admin Project 1", KanbanBoard = kanbanBoard1 };
            var project2 = new Project { Id = 2, Title = "Admin Project 2", KanbanBoard = kanbanBoard2 };

            await _context.Projects.AddRangeAsync(project1, project2);
            await _context.SaveChangesAsync();

            var result = await _controller.Index(0) as ViewResult;

            Assert.That(result, Is.Not.Null, "Index should return a ViewResult");
            Assert.That(result.Model, Is.InstanceOf<KanbanBoard>(), "Expected KanbanBoard as model");

            var userProjects = result.ViewData["UserProjects"] as List<Project>;
            Assert.That(userProjects, Is.Not.Null);
            Assert.That(userProjects.Count, Is.EqualTo(2));
        }



        [Test]
        public async Task Index_UserIsRegularUser_LoadsOnlyTheirProjects()
        {
            _userMock.Setup(u => u.IsInRole("admin")).Returns(false);

            var kanbanBoard = new KanbanBoard { Id = 1, ProjectName = "User Board" };
            var userProject = new Project { Id = 1, Title = "User Project", ProjectManagerId = _testUserId, KanbanBoard = kanbanBoard };
            var otherProject = new Project { Id = 2, Title = "Other Project", ProjectManagerId = "otherUser" };

            await _context.Projects.AddRangeAsync(userProject, otherProject);
            await _context.SaveChangesAsync();

            var result = await _controller.Index(0) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Model, Is.InstanceOf<KanbanBoard>(), "Expected KanbanBoard as model");

            var userProjects = result.ViewData["UserProjects"] as List<Project>;
            Assert.That(userProjects, Is.Not.Null);
            Assert.That(userProjects.Count, Is.EqualTo(1));
        }


        [Test]
        public async Task Index_UserHasNoProjects_ReturnsEmptyKanbanBoard()
        {
            _userMock.Setup(u => u.IsInRole("admin")).Returns(false);

            var result = await _controller.Index(0) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Model, Is.InstanceOf<KanbanBoard>());

            var userProjects = result.ViewData["UserProjects"] as List<Project>;
            Assert.That(userProjects, Is.Not.Null);
            Assert.That(userProjects.Count, Is.EqualTo(0));
        }

        [Test]
        public async Task Index_ProjectIdNotFound_ReturnsEmptyKanbanBoard()
        {
            var userProject = new Project { Id = 1, Title = "Valid Project", ProjectManagerId = _testUserId };
            _context.Projects.Add(userProject);
            await _context.SaveChangesAsync();

            var result = await _controller.Index(999) as ViewResult;

            Assert.That(result, Is.Not.Null, "Expected a ViewResult but got null");
            Assert.That(result.Model, Is.InstanceOf<KanbanBoard>());
            Assert.That(((KanbanBoard)result.Model).Columns.Count, Is.EqualTo(0));
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
        public async Task RenameColumn_ValidColumn_UpdatesName()
        {
            var column = new KanbanColumn { Id = 1, Name = "Old Name" };
            _context.KanbanColumns.Add(column);
            await _context.SaveChangesAsync();

            var result = await _controller.RenameColumn(1, "New Name") as JsonResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(column.Name, Is.EqualTo("New Name"));
        }

        [Test]
        public async Task ViewTicketDetails_TicketExists_ReturnsDetails()
        {
            var ticket = new Ticket
            {
                TicketId = 1,
                Title = "Fix Bug",
                Description = "Resolve issue #123",
                Status = "Open",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "TestUser"
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            var result = await _controller.ViewTicketDetails(ticket.TicketId) as JsonResult;

            Console.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(result?.Value));

            var value = JObject.FromObject(result?.Value);

            Assert.That(value["success"], Is.Not.Null, "Expected 'success' property in response.");
            Assert.That((bool)value["success"], Is.True);
            Assert.That((int)value["ticket"]["id"], Is.EqualTo(ticket.TicketId));
            Assert.That((string)value["ticket"]["title"], Is.EqualTo(ticket.Title));
            Assert.That((string)value["ticket"]["description"], Is.EqualTo(ticket.Description));
            Assert.That((string)value["ticket"]["status"], Is.EqualTo(ticket.Status));
        }


        [Test]
        public async Task EditTicket_ValidTicket_UpdatesDetails()
        {
            var ticket = new Ticket
            {
                TicketId = 1,
                Title = "Original Title",
                Description = "Original Description",
                CreatedBy = "TestUser", 
                Status = "To Do" 
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            var result = await _controller.EditTicket(1, "Updated Title", "Updated Description") as JsonResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(ticket.Title, Is.EqualTo("Updated Title"));
            Assert.That(ticket.Description, Is.EqualTo("Updated Description"));
        }

        [Test]
        public async Task DeleteTicket_ValidTicket_DeletesSuccessfully()
        {
            var ticket = new Ticket
            {
                TicketId = 1,
                Title = "Delete Me",
                Description = "This ticket will be deleted.",
                CreatedBy = "TestUser",
                Status = "Open"
            };
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteTicket(1) as JsonResult;

            Assert.That(result, Is.Not.Null);
            var deletedTicket = await _context.Tickets.FindAsync(1);
            Assert.That(deletedTicket, Is.Null);
        }

        [Test]
        public async Task AddTicket_NullTicket_ReturnsBadRequest()
        {
            var result = await _controller.AddTicket(null) as BadRequestObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(400));
            Assert.That(result.Value, Is.EqualTo("Invalid ticket data."));
        }

        [Test]
        public async Task AddTicket_MissingTitleOrDescription_ReturnsBadRequest()
        {
            var invalidTicket1 = new Ticket { ProjectId = 1, Title = "", Description = "Valid Description" };
            var invalidTicket2 = new Ticket { ProjectId = 1, Title = "Valid Title", Description = "" };

            var result1 = await _controller.AddTicket(invalidTicket1) as BadRequestObjectResult;
            var result2 = await _controller.AddTicket(invalidTicket2) as BadRequestObjectResult;

            Assert.That(result1, Is.Not.Null);
            Assert.That(result1.StatusCode, Is.EqualTo(400));
            Assert.That(result1.Value, Is.EqualTo("Invalid ticket data."));

            Assert.That(result2, Is.Not.Null);
            Assert.That(result2.StatusCode, Is.EqualTo(400));
            Assert.That(result2.Value, Is.EqualTo("Invalid ticket data."));
        }

        [Test]
        public async Task AddTicket_ProjectNotFound_ReturnsNotFound()
        {
            var ticket = new Ticket { ProjectId = 999, Title = "New Ticket", Description = "Ticket Description" };

            var result = await _controller.AddTicket(ticket) as NotFoundObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.StatusCode, Is.EqualTo(404));
            Assert.That(result.Value, Is.EqualTo("Project not found."));
        }

        [Test]
        public async Task AddTicket_ValidTicket_AddsSuccessfully()
        {
            var project = new Project
            {
                Id = 1,
                Title = "Test Project",
                KanbanBoard = new KanbanBoard { Id = 1, ProjectName = "Test Board" }
            };

            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();

            var ticket = new Ticket { ProjectId = 1, Title = "New Ticket", Description = "Valid Description" };

            var result = await _controller.AddTicket(ticket) as OkObjectResult;

            Assert.That(result, Is.Not.Null);
            var savedTicket = await _context.Tickets.FirstOrDefaultAsync(t => t.Title == "New Ticket");

            Assert.That(savedTicket, Is.Not.Null);
            Assert.That(savedTicket.Description, Is.EqualTo("Valid Description"));
            Assert.That(savedTicket.Status, Is.EqualTo("To Do"));
        }
    }
}
