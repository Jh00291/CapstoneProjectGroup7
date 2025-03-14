using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using TicketSystemWeb.Controllers;
using TicketSystemWeb.Data;
using TicketSystemWeb.Models.Employee;
using TicketSystemWeb.Models.ProjectManagement.Group;
using TicketSystemWeb.Models.ProjectManagement.Project;
using TicketSystemWeb.ViewModels;

namespace TicketSystemWeb.Tests.Controllers
{
    [TestFixture]
    public class ProjectManagementControllerTests : IDisposable
    {
        private Mock<UserManager<Employee>> _userManagerMock;
        private TicketDBContext _context;
        private ProjectManagementController _controller;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<TicketDBContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new TicketDBContext(options);

            _userManagerMock = new Mock<UserManager<Employee>>(
                Mock.Of<IUserStore<Employee>>(), null, null, null, null, null, null, null, null);

            var testUser = new Employee { Id = "testUser", UserName = "Test User" };
            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                            .ReturnsAsync(testUser);

            _userManagerMock.Setup(um => um.IsInRoleAsync(It.IsAny<Employee>(), "admin"))
                            .ReturnsAsync(false);

            _controller = new ProjectManagementController(_context, _userManagerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Dispose();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _context?.Dispose();
            _controller?.Dispose();
        }

        [Test]
        public async Task Management_ReturnsViewWithProjectsAndGroups()
        {
            var result = await _controller.Management() as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Model, Is.TypeOf<ManagementViewModel>());
        }

        [Test]
        public async Task CreateGroup_ValidModel_AddsGroupAndRedirects()
        {
            var model = new AddGroupViewModel { Name = "NewGroup", SelectedManagerId = "manager1" };

            _context.Users.Add(new Employee { Id = "manager1", UserName = "ManagerUser" });
            await _context.SaveChangesAsync();

            var result = await _controller.CreateGroup(model);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            Assert.That(((RedirectToActionResult)result).ActionName, Is.EqualTo("Management"));
        }

        // -- Update Group --


        [Test]
        public async Task UpdateGroup_WhenGroupDoesNotExist_ReturnsNotFound()
        {
            var model = new EditGroupViewModel { GroupId = 999, Name = "Updated Group" };

            var result = await _controller.UpdateGroup(model);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task UpdateGroup_WhenUserIsNotAdminOrManager_ReturnsForbid()
        {
            var group = new Group { Id = 1, Name = "Group1", ManagerId = "manager1" };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            var normalUser = new Employee { Id = "user1" };
            var userClaims = new Mock<System.Security.Claims.ClaimsPrincipal>();

            _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                            .ReturnsAsync(normalUser);
            _userManagerMock.Setup(u => u.IsInRoleAsync(normalUser, "admin")).ReturnsAsync(false);
            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                            .Returns("user1");

            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaims.Object }
            };

            var model = new EditGroupViewModel { GroupId = 1, Name = "Updated Group", SelectedManagerId = "manager1", SelectedEmployeeIds = new List<string>() };
            var result = await _controller.UpdateGroup(model);

            Assert.That(result, Is.TypeOf<ForbidResult>());
        }


        // --- Edit Group ---
        [Test]
        public async Task EditGroup_ValidId_ReturnsPartialView()
        {
            var group = new Group { Id = 1, Name = "TestGroup", ManagerId = "123" };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            var result = await _controller.EditGroup(1);

            Assert.That(result, Is.TypeOf<PartialViewResult>());
            Assert.That(((PartialViewResult)result).ViewName, Is.EqualTo("EditGroup"));
        }

        [Test]
        public async Task GetEditGroup_NonExistentGroup_ReturnsNotFound()
        {
            var result = await _controller.EditGroup(999);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        // --- Delete Group ---

        [Test]
        public async Task DeleteGroup_NonAdminNonManager_ReturnsForbid()
        {
            var group = new Group { Id = 1, Name = "Test Group", ManagerId = "manager1" };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            var normalUser = new Employee { Id = "user1" };
            _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                            .ReturnsAsync(normalUser);
            _userManagerMock.Setup(u => u.IsInRoleAsync(normalUser, "admin")).ReturnsAsync(false);

            var result = await _controller.DeleteGroup(1);

            Assert.That(result, Is.TypeOf<ForbidResult>());
        }

        // --- Create Project ---
        [Test]
        public async Task CreateProject_ValidModel_AddsProjectAndRedirects()
        {
            var model = new AddProjectViewModel
            {
                Title = "New Project",
                Description = "Test Project",
                ProjectManagerId = "manager1",
                GroupIds = new List<int> { 1, 2 }
            };

            _context.Users.Add(new Employee { Id = "manager1", UserName = "Test Manager" });
            await _context.SaveChangesAsync();

            var result = await _controller.CreateProject(model);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());

            var createdProject = _context.Projects.FirstOrDefault(p => p.Title == "New Project");
            Assert.That(createdProject, Is.Not.Null);
        }

        [Test]
        public async Task CreateProject_WhenGroupIdsAreNull_AddsProjectWithoutGroups()
        {
            var model = new AddProjectViewModel
            {
                Title = "Project Without Groups",
                Description = "A project without any group",
                ProjectManagerId = "manager1",
                GroupIds = null
            };

            _context.Users.Add(new Employee { Id = "manager1", UserName = "Test Manager" });
            await _context.SaveChangesAsync();

            var result = await _controller.CreateProject(model);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
        }


        // --- Update Project ---
        [Test]
        public async Task UpdateProject_NonExistentProject_ReturnsNotFound()
        {
            var model = new EditProjectViewModel { ProjectId = 999 };

            var result = await _controller.UpdateProject(model);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task UpdateProject_NonAdminNonManager_ReturnsForbid()
        {
            var project = new Project { Id = 1, Title = "Test Project", ProjectManagerId = "manager1" };
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var normalUser = new Employee { Id = "normalUser" };
            _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                            .ReturnsAsync(normalUser);
            _userManagerMock.Setup(u => u.IsInRoleAsync(normalUser, "admin")).ReturnsAsync(false);

            var model = new EditProjectViewModel { ProjectId = 1, Title = "Updated Title" };
            var result = await _controller.UpdateProject(model);

            Assert.That(result, Is.TypeOf<ForbidResult>());
        }


        // -- Delete Project --

        [Test]
        public async Task DeleteProject_UnauthorizedUser_ReturnsUnauthorized()
        {
            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                            .ReturnsAsync((Employee)null);

            var result = await _controller.DeleteProject(1);

            Assert.That(result, Is.TypeOf<UnauthorizedResult>());
        }

        [Test]
        public async Task DeleteProject_NonAdminNonManager_ReturnsForbid()
        {
            var project = new Project { Id = 1, Title = "Test Project", ProjectManagerId = "manager1" };
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var normalUser = new Employee { Id = "normalUser" };
            _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                            .ReturnsAsync(normalUser);
            _userManagerMock.Setup(u => u.IsInRoleAsync(normalUser, "admin")).ReturnsAsync(false);

            var result = await _controller.DeleteProject(1);

            Assert.That(result, Is.TypeOf<ForbidResult>());
        }

        [Test]
        public async Task DeleteProject_WhenAdminDeletesOwnProject_RemovesProject()
        {
            var project = new Project { Id = 1, Title = "Admin's Project", ProjectManagerId = "adminUser" };
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var adminUser = new Employee { Id = "adminUser" };
            _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                            .ReturnsAsync(adminUser);
            _userManagerMock.Setup(u => u.IsInRoleAsync(adminUser, "admin")).ReturnsAsync(true);

            var result = await _controller.DeleteProject(1);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            Assert.That(_context.Projects.Find(1), Is.Null);
        }


        [Test]
        public async Task DeleteProject_AdminOrProjectManager_DeletesProjectAndRedirects()
        {
            var project = new Project { Id = 1, Title = "To Be Deleted", ProjectManagerId = "adminUser" };
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var adminUser = new Employee { Id = "adminUser" };
            _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                            .ReturnsAsync(adminUser);
            _userManagerMock.Setup(u => u.IsInRoleAsync(adminUser, "admin")).ReturnsAsync(true);

            var result = await _controller.DeleteProject(1);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var redirectResult = (RedirectToActionResult)result;
            Assert.That(redirectResult.ActionName, Is.EqualTo("Management"));
        }


        // -- Edit Project --

        [Test]
        public async Task EditProject_WhenProjectExists_ReturnsPartialView()
        {
            var project = new Project { Id = 1, Title = "Test Project", ProjectManagerId = "manager1" };
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var result = await _controller.EditProject(1);

            Assert.That(result, Is.TypeOf<PartialViewResult>());
            Assert.That(((PartialViewResult)result).ViewName, Is.EqualTo("Projects/EditProject"));
        }

        [Test]
        public async Task EditProject_WhenProjectDoesNotExist_ReturnsNotFound()
        {
            var result = await _controller.EditProject(999);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task CreateProject_WhenModelIsInvalid_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("Title", "Required");

            var model = new AddProjectViewModel { Description = "Test" };
            var result = await _controller.CreateProject(model);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CreateProject_WhenProjectTitleExists_ReturnsBadRequest()
        {
            _context.Projects.Add(new Project { Title = "Existing Project", ProjectManagerId = "manager1" });
            await _context.SaveChangesAsync();

            var model = new AddProjectViewModel
            {
                Title = "Existing Project",
                Description = "Test Project",
                ProjectManagerId = "manager1"
            };

            var result = await _controller.CreateProject(model);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task DeleteProject_WhenProjectDoesNotExist_ReturnsNotFound()
        {
            var result = await _controller.DeleteProject(999);

            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task UpdateProject_WhenUserIsNull_ReturnsUnauthorized()
        {
            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                            .ReturnsAsync((Employee)null);

            var model = new EditProjectViewModel { ProjectId = 1, Title = "Updated Title" };
            var result = await _controller.UpdateProject(model);

            Assert.That(result, Is.TypeOf<UnauthorizedResult>());
        }

        [Test]
        public async Task ApproveGroupAddition_WhenUserIsNull_ReturnsBadRequest()
        {
            _userManagerMock.Setup(um => um.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                            .ReturnsAsync((Employee)null);

            var result = await _controller.ApproveGroupAddition(1, 1);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task ApproveGroupAddition_WhenGroupRequestNotFound_ReturnsNotFound()
        {
            var result = await _controller.ApproveGroupAddition(1, 999);

            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task ApproveGroupAddition_WhenUserIsNotGroupManager_ReturnsForbid()
        {
            var group = new Group { Id = 1, Name = "Group1", ManagerId = "manager1" };
            var projectGroup = new ProjectGroup { ProjectId = 1, GroupId = 1, IsApproved = false, Group = group };
            _context.Groups.Add(group);
            _context.ProjectGroups.Add(projectGroup);
            await _context.SaveChangesAsync();

            var normalUser = new Employee { Id = "user1" };
            _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                            .ReturnsAsync(normalUser);

            var result = await _controller.ApproveGroupAddition(1, 1);

            Assert.That(result, Is.TypeOf<JsonResult>());
            var jsonResult = result as JsonResult;
            Assert.That(jsonResult.StatusCode, Is.EqualTo(StatusCodes.Status403Forbidden));
        }
    }
}
