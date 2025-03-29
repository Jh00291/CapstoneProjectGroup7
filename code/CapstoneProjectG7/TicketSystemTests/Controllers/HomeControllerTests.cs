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
using TicketSystemWeb.Models.Employee;
using TicketSystemWeb.Models.ProjectManagement.Group;

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
            _userMock.Setup(u => u.Identity.Name).Returns("TestUser");
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
        public async Task EditTicket_ValidTicket_UpdatesDetails_AndAssignment()
        {
            var employee = new Employee
            {
                Id = "user123",
                UserName = "EmployeeUser"
            };
            _context.Employees.Add(employee);

            var ticket = new Ticket
            {
                TicketId = 1,
                Title = "Original Title",
                Description = "Original Description",
                CreatedBy = "TestUser",
                Status = "To Do",
                AssignedToId = null
            };
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();


            var result = await _controller.EditTicket(1, "Updated Title", "Updated Description", employee.Id) as JsonResult;


            Assert.That(result, Is.Not.Null);
            Assert.That(ticket.Title, Is.EqualTo("Updated Title"));
            Assert.That(ticket.Description, Is.EqualTo("Updated Description"));
            Assert.That(ticket.AssignedToId, Is.EqualTo(employee.Id));
        }

        [Test]
        public async Task EditTicket_WithNullAssignedToId_UnassignsTheTicket()
        {
            var employee = new Employee
            {
                Id = "user123",
                UserName = "EmployeeUser"
            };
            _context.Employees.Add(employee);

            var ticket = new Ticket
            {
                TicketId = 2,
                Title = "Assigned Ticket",
                Description = "This ticket has an assignee",
                CreatedBy = "TestUser",
                Status = "To Do",
                AssignedToId = employee.Id
            };
            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            var result = await _controller.EditTicket(2, "New Title", "New Description", null) as JsonResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(ticket.Title, Is.EqualTo("New Title"));
            Assert.That(ticket.Description, Is.EqualTo("New Description"));
            Assert.That(ticket.AssignedToId, Is.Null);
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
            var board = new KanbanBoard { Id = 1, ProjectName = "Test Board", Columns = new List<KanbanColumn>() };
            var column = new KanbanColumn { Id = 1, Name = "To Do", Order = 1, KanbanBoardId = 1, KanbanBoard = board };
            board.Columns.Add(column);

            var project = new Project
            {
                Id = 1,
                Title = "Test Project",
                KanbanBoard = board
            };

            await _context.KanbanBoards.AddAsync(board);
            await _context.KanbanColumns.AddAsync(column);
            await _context.Projects.AddAsync(project);
            await _context.SaveChangesAsync();

            var ticket = new Ticket { ProjectId = 1, Title = "New Ticket", Description = "Valid Description" };
            var result = await _controller.AddTicket(ticket) as OkObjectResult;

            Assert.That(result, Is.Not.Null, "AddTicket did not return a successful result");
            var savedTicket = await _context.Tickets.FirstOrDefaultAsync(t => t.Title == "New Ticket");

            Assert.That(savedTicket, Is.Not.Null);
            Assert.That(savedTicket.Description, Is.EqualTo("Valid Description"));
            Assert.That(savedTicket.Status, Is.EqualTo("To Do"));
            Assert.That(savedTicket.CreatedBy, Is.EqualTo("TestUser"));
        }






        [Test]
        public async Task AddTicket_NoColumnsInKanbanBoard_ReturnsBadRequest()
        {
            var board = new KanbanBoard { Id = 1, ProjectName = "Empty Board", Columns = new List<KanbanColumn>() };
            var project = new Project { Id = 1, Title = "Project with Empty Board", KanbanBoard = board };

            await _context.Projects.AddAsync(project);
            await _context.KanbanBoards.AddAsync(board);
            await _context.SaveChangesAsync();

            var ticket = new Ticket { ProjectId = 1, Title = "Ticket", Description = "No column test" };
            var result = await _controller.AddTicket(ticket) as BadRequestObjectResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Value, Is.EqualTo("No columns available in the Kanban board."));
        }

        [Test]
        public async Task MoveTicket_FromFirstColumn_Unassigned_AssignsUser()
        {
            var board = new KanbanBoard { Id = 1, ProjectName = "Board 1" };

            var firstColumn = new KanbanColumn
            {
                Id = 1,
                Name = "To Do",
                Order = 1,
                KanbanBoardId = board.Id,
                KanbanBoard = board
            };

            var secondColumn = new KanbanColumn
            {
                Id = 2,
                Name = "In Progress",
                Order = 2,
                KanbanBoardId = board.Id,
                KanbanBoard = board
            };

            var project = new Project
            {
                Id = 1,
                Title = "Project A",
                KanbanBoard = board
            };

            var ticket = new Ticket
            {
                TicketId = 1,
                Title = "Fix this bug",
                Description = "Bug desc",
                CreatedBy = "TestUser",
                ProjectId = project.Id,
                Status = "To Do",
                AssignedToId = null
            };

            await _context.KanbanBoards.AddAsync(board);
            await _context.KanbanColumns.AddRangeAsync(firstColumn, secondColumn);
            await _context.Projects.AddAsync(project);
            await _context.Tickets.AddAsync(ticket);
            await _context.SaveChangesAsync();

            var result = await _controller.MoveTicket(ticket.TicketId, secondColumn.Id) as JsonResult;

            var updated = await _context.Tickets.FindAsync(ticket.TicketId);

            Assert.That(result, Is.Not.Null);
            Assert.That(updated.Status, Is.EqualTo("In Progress"));
            Assert.That(updated.AssignedToId, Is.EqualTo(_testUserId));
        }

        [Test]
        public async Task MoveTicket_ToFirstColumn_Assigned_UnassignsUser()
        {
            var column = new KanbanColumn { Id = 2, Name = "To Do", Order = 1 };
            var board = new KanbanBoard
            {
                Id = 2,
                ProjectName = "Another Board",
                Columns = new List<KanbanColumn> { column }
            };
            column.KanbanBoardId = board.Id;

            var project = new Project
            {
                Id = 2,
                Title = "Unassign Test Project",
                KanbanBoard = board
            };

            var ticket = new Ticket
            {
                TicketId = 2,
                Title = "Assigned Ticket",
                Status = "In Progress",
                ProjectId = project.Id,
                AssignedToId = _testUserId,
                CreatedBy = "TestUser",
                Description = "Test description"
            };

            await _context.Projects.AddAsync(project);
            await _context.KanbanBoards.AddAsync(board);
            await _context.KanbanColumns.AddAsync(column);
            await _context.Tickets.AddAsync(ticket);
            await _context.SaveChangesAsync();

            var result = await _controller.MoveTicket(ticket.TicketId, column.Id) as JsonResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(ticket.AssignedToId, Is.Null);
        }

        [Test]
        public async Task AddTicket_SetsStatusToFirstColumnName()
        {
            var board = new KanbanBoard { Id = 1, ProjectName = "Test Board", Columns = new List<KanbanColumn>() };
            var column = new KanbanColumn { Id = 1, Name = "To Do", Order = 1, KanbanBoard = board };
            board.Columns.Add(column);

            var project = new Project
            {
                Id = 1,
                Title = "Test Project",
                KanbanBoard = board
            };

            await _context.Projects.AddAsync(project);
            await _context.KanbanBoards.AddAsync(board);
            await _context.KanbanColumns.AddAsync(column);
            await _context.SaveChangesAsync();

            var ticket = new Ticket { ProjectId = 1, Title = "New Ticket", Description = "Description" };
            var result = await _controller.AddTicket(ticket) as OkObjectResult;

            var savedTicket = await _context.Tickets.FirstOrDefaultAsync(t => t.Title == "New Ticket");

            Assert.That(savedTicket, Is.Not.Null);
            Assert.That(savedTicket.Status, Is.EqualTo("To Do"));
        }


        [Test]
        public async Task ViewTicketDetails_WithHistoryAndComments_ReturnsExpectedJson()
        {
            var ticket = new Ticket
            {
                TicketId = 99,
                Title = "History Test",
                Description = "Testing",
                Status = "Open",
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "Tester",
                History = new List<TicketHistory>
                {
                    new TicketHistory { Action = "Created", Timestamp = DateTime.UtcNow.AddMinutes(-5), PerformedBy = "Tester" }
                },
                Comments = new List<TicketComment>
                {
                    new TicketComment { CommentText = "Looks good", AuthorName = "Reviewer", CreatedAt = DateTime.UtcNow }
                }
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            var result = await _controller.ViewTicketDetails(ticket.TicketId) as JsonResult;
            var jObject = JObject.FromObject(result!.Value);

            Assert.That(jObject["success"]!.Value<bool>(), Is.True);
            Assert.That(jObject["ticket"]!["title"]!.Value<string>(), Is.EqualTo("History Test"));
            Assert.That(jObject["ticket"]!["comments"]![0]!["author"]!.Value<string>(), Is.EqualTo("Reviewer"));
            Assert.That(jObject["ticket"]!["history"]![0]!["performedBy"]!.Value<string>(), Is.EqualTo("Tester"));
        }


        [Test]
        public async Task AddTicketComment_ValidInput_AddsCommentSuccessfully()
        {
            var ticket = new Ticket
            {
                TicketId = 55,
                Title = "Needs Comment",
                Description = "Add one!",
                CreatedBy = "Tester",
                Status = "To Do"
            };

            _context.Tickets.Add(ticket);
            await _context.SaveChangesAsync();

            var result = await _controller.AddTicketComment(55, "This is a comment") as JsonResult;

            var savedComment = await _context.TicketComments.FirstOrDefaultAsync(c => c.TicketId == 55);
            Assert.That(result, Is.Not.Null);
            Assert.That(savedComment, Is.Not.Null);
            Assert.That(savedComment.CommentText, Is.EqualTo("This is a comment"));
            Assert.That(savedComment.AuthorName, Is.EqualTo("TestUser"));
        }

        [Test]
        public async Task GetProjectEmployees_ReturnsEmployeeList()
        {
            var employee = new Employee { Id = "emp1", UserName = "Alice" };
            var group = new Group { Id = 1, Name = "Team Alpha" };
            var project = new Project { Id = 1, Title = "Project X" };

            var employeeGroup = new EmployeeGroup { Employee = employee, Group = group };
            var projectGroup = new ProjectGroup { Group = group, Project = project };

            group.EmployeeGroups = new List<EmployeeGroup> { employeeGroup };
            group.ProjectGroups = new List<ProjectGroup> { projectGroup };

            _context.Employees.Add(employee);
            _context.Groups.Add(group);
            _context.Projects.Add(project);
            _context.EmployeeGroups.Add(employeeGroup);
            _context.ProjectGroups.Add(projectGroup);
            await _context.SaveChangesAsync();

            var result = await _controller.GetProjectEmployees(1) as JsonResult;
            var employees = result?.Value as IEnumerable<object>;

            Assert.That(employees, Is.Not.Null);
            Assert.That(employees.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task DeleteColumn_WithTickets_ReassignsToFirstColumn()
        {
            var board = new KanbanBoard { Id = 1, ProjectName = "Test Board" };

            var column1 = new KanbanColumn
            {
                Id = 1,
                Name = "To Do",
                Order = 1,
                KanbanBoardId = board.Id,
                KanbanBoard = board
            };

            var column2 = new KanbanColumn
            {
                Id = 2,
                Name = "In Progress",
                Order = 2,
                KanbanBoardId = board.Id,
                KanbanBoard = board
            };

            var project = new Project
            {
                Id = 1,
                Title = "Test Project",
                KanbanBoard = board
            };

            var ticket = new Ticket
            {
                TicketId = 1,
                Title = "Test Ticket",
                Description = "Bug",
                Status = "In Progress",
                CreatedBy = "TestUser",
                ProjectId = project.Id
            };

            await _context.KanbanBoards.AddAsync(board);
            await _context.KanbanColumns.AddRangeAsync(column1, column2);
            await _context.Projects.AddAsync(project);
            await _context.Tickets.AddAsync(ticket);
            await _context.SaveChangesAsync();

            var result = await _controller.DeleteColumn(column2.Id) as JsonResult;

            var updatedTicket = await _context.Tickets.FindAsync(ticket.TicketId);
            Assert.That(updatedTicket.Status, Is.EqualTo("To Do"));
            Assert.That(result, Is.Not.Null);
        }

    }
}
