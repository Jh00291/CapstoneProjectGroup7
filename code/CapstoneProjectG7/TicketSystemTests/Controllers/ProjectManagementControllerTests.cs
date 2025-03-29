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
using Newtonsoft.Json;

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

        [Test]
        public async Task CreateProject_WhenProjectManagerIsInvalid_ReturnsBadRequest()
        {
            var model = new AddProjectViewModel
            {
                Title = "Invalid Manager Project",
                Description = "No such manager",
                ProjectManagerId = "nonexistent"
            };

            var result = await _controller.CreateProject(model);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequest = result as BadRequestObjectResult;
            Assert.That(badRequest.Value, Is.EqualTo("Invalid Project Manager"));
        }

        [Test]
        public async Task CreateProject_WithGroups_CreatesCorrectApprovalStatus()
        {
            var manager = new Employee { Id = "manager1", UserName = "Manager1" };
            var otherManager = new Employee { Id = "manager2", UserName = "Manager2" };
            _context.Users.AddRange(manager, otherManager);
            await _context.SaveChangesAsync();

            var group1 = new Group { Id = 1, Name = "Group1", ManagerId = "manager1" };
            var group2 = new Group { Id = 2, Name = "Group2", ManagerId = "manager2" };
            _context.Groups.AddRange(group1, group2);
            await _context.SaveChangesAsync();

            var model = new AddProjectViewModel
            {
                Title = "Project With Groups",
                Description = "Check approval logic",
                ProjectManagerId = "manager1",
                GroupIds = new List<int> { 1, 2 }
            };

            var result = await _controller.CreateProject(model);

            var pg1 = await _context.ProjectGroups.FirstOrDefaultAsync(pg => pg.GroupId == 1);
            var pg2 = await _context.ProjectGroups.FirstOrDefaultAsync(pg => pg.GroupId == 2);

            Assert.That(pg1.IsApproved, Is.True);
            Assert.That(pg2.IsApproved, Is.False);
        }

        [Test]
        public async Task UpdateProject_WhenModelIsInvalid_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("Title", "Required");
            var model = new EditProjectViewModel { ProjectId = 1 };

            var result = await _controller.UpdateProject(model);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequest = result as BadRequestObjectResult;
            Assert.That(badRequest.Value, Is.EqualTo("Invalid project details."));
        }

        [Test]
        public async Task UpdateProject_ChangesManagerAndGroups_SuccessfullyUpdates()
        {
            var manager1 = new Employee { Id = "manager1", UserName = "Manager1" };
            var manager2 = new Employee { Id = "manager2", UserName = "Manager2" };
            var groupOld = new Group { Id = 1, Name = "OldGroup" };
            var groupNew = new Group { Id = 2, Name = "NewGroup" };

            _context.Users.AddRange(manager1, manager2);
            _context.Groups.AddRange(groupOld, groupNew);
            await _context.SaveChangesAsync();

            var project = new Project
            {
                Id = 1,
                Title = "Original Title",
                Description = "Original Desc",
                ProjectManagerId = "manager1"
            };
            _context.Projects.Add(project);
            _context.ProjectGroups.Add(new ProjectGroup { ProjectId = 1, GroupId = 1 });
            await _context.SaveChangesAsync();

            _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                            .ReturnsAsync(manager1);
            _userManagerMock.Setup(u => u.IsInRoleAsync(manager1, "Admin"))
                            .ReturnsAsync(true);

            var model = new EditProjectViewModel
            {
                ProjectId = 1,
                Title = "Updated Title",
                Description = "Updated Desc",
                ProjectManagerId = "manager2",
                SelectedGroupIds = new List<int> { 2 }
            };

            var result = await _controller.UpdateProject(model);

            var updated = await _context.Projects.Include(p => p.ProjectGroups).FirstAsync(p => p.Id == 1);
            Assert.That(updated.Title, Is.EqualTo("Updated Title"));
            Assert.That(updated.ProjectManagerId, Is.EqualTo("manager2"));
            Assert.That(updated.ProjectGroups.Any(pg => pg.GroupId == 2), Is.True);
            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
        }

        [Test]
        public async Task CreateGroup_WithEmployees_AddsEmployeeGroups()
        {
            var manager = new Employee { Id = "manager1", UserName = "Manager" };
            var emp1 = new Employee { Id = "emp1", UserName = "Emp One" };
            var emp2 = new Employee { Id = "emp2", UserName = "Emp Two" };
            _context.Users.AddRange(manager, emp1, emp2);
            await _context.SaveChangesAsync();

            var model = new AddGroupViewModel
            {
                Name = "Group with Members",
                SelectedManagerId = "manager1",
                SelectedEmployeeIds = new List<string> { "emp1", "emp2" }
            };

            var result = await _controller.CreateGroup(model);

            var group = await _context.Groups.Include(g => g.EmployeeGroups).FirstOrDefaultAsync(g => g.Name == "Group with Members");

            Assert.That(group.EmployeeGroups.Count, Is.EqualTo(2));
            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
        }

        [Test]
        public async Task EditProject_IncludesSelectedGroupIdsInViewModel()
        {
            var group = new Group { Id = 1, Name = "Group1" };
            var project = new Project
            {
                Id = 1,
                Title = "Test Project",
                Description = "Desc",
                ProjectManagerId = "manager1",
                ProjectGroups = new List<ProjectGroup>
        {
            new ProjectGroup { GroupId = 1, ProjectId = 1 }
        }
            };

            _context.Groups.Add(group);
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var result = await _controller.EditProject(1);
            var viewResult = result as PartialViewResult;
            var viewModel = viewResult?.Model as EditProjectViewModel;

            Assert.That(viewModel.SelectedGroupIds.Contains(1), Is.True);
        }

        [Test]
        public async Task UpdateGroup_ChangesNameManagerAndEmployees()
        {
            var manager = new Employee { Id = "manager1" };
            var newManager = new Employee { Id = "manager2" };
            var emp1 = new Employee { Id = "emp1" };
            var emp2 = new Employee { Id = "emp2" };

            _context.Users.AddRange(manager, newManager, emp1, emp2);
            var group = new Group
            {
                Id = 1,
                Name = "Old Group",
                ManagerId = "manager1"
            };
            _context.Groups.Add(group);
            _context.EmployeeGroups.Add(new EmployeeGroup { GroupId = 1, EmployeeId = "emp1" });
            await _context.SaveChangesAsync();

            var model = new EditGroupViewModel
            {
                GroupId = 1,
                Name = "Updated Group",
                SelectedManagerId = "manager2",
                SelectedEmployeeIds = new List<string> { "emp2" }
            };

            var userClaims = new Mock<System.Security.Claims.ClaimsPrincipal>();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = userClaims.Object }
            };

            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                            .Returns("manager1");
            _userManagerMock.Setup(u => u.IsInRoleAsync(It.IsAny<Employee>(), "admin"))
                            .ReturnsAsync(false);

            var result = await _controller.UpdateGroup(model);

            var updatedGroup = await _context.Groups.FindAsync(1);
            var updatedEmployees = _context.EmployeeGroups.Where(eg => eg.GroupId == 1).ToList();

            Assert.That(updatedGroup.Name, Is.EqualTo("Updated Group"));
            Assert.That(updatedGroup.ManagerId, Is.EqualTo("manager2"));
            Assert.That(updatedEmployees.Count, Is.EqualTo(1));
            Assert.That(updatedEmployees.First().EmployeeId, Is.EqualTo("emp2"));
            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
        }

        [Test]
        public async Task DeleteGroup_RemovesGroupAndEmployeeGroups()
        {
            var manager = new Employee { Id = "manager1" };
            _context.Users.Add(manager);
            var group = new Group
            {
                Id = 1,
                Name = "Deletable Group",
                ManagerId = "manager1",
                EmployeeGroups = new List<EmployeeGroup>
                {
                    new EmployeeGroup { EmployeeId = "emp1" },
                    new EmployeeGroup { EmployeeId = "emp2" }
                }
            };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                            .ReturnsAsync(manager);
            _userManagerMock.Setup(u => u.IsInRoleAsync(manager, "admin"))
                            .ReturnsAsync(false);

            var result = await _controller.DeleteGroup(1);

            var deletedGroup = await _context.Groups.FindAsync(1);
            var relatedEmployees = _context.EmployeeGroups.Where(e => e.GroupId == 1).ToList();

            Assert.That(deletedGroup, Is.Null);
            Assert.That(relatedEmployees.Count, Is.EqualTo(0));
            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
        }

        [Test]
        public async Task GetGroups_ReturnsGroupsAsJson()
        {
            _context.Groups.Add(new Group { Id = 1, Name = "Group 1" });
            await _context.SaveChangesAsync();

            var result = await _controller.GetGroups();
            Assert.That(result, Is.TypeOf<JsonResult>());

            var json = result as JsonResult;
            var groups = json.Value as IEnumerable<object>;

            Assert.That(groups, Is.Not.Null);
            Assert.That(groups.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task GetEmployees_ReturnsEmployeesAsJson()
        {
            _context.Users.Add(new Employee { Id = "e1", UserName = "Alice" });
            await _context.SaveChangesAsync();

            var result = await _controller.GetEmployees();
            Assert.That(result, Is.TypeOf<JsonResult>());

            var json = result as JsonResult;
            var employees = json.Value as IEnumerable<object>;

            Assert.That(employees, Is.Not.Null);
            Assert.That(employees.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task ApproveGroupAddition_Succeeds_WhenUserIsManager()
        {
            var manager = new Employee { Id = "manager1" };
            var group = new Group { Id = 1, Name = "Group1", ManagerId = "manager1" };
            var projectGroup = new ProjectGroup { ProjectId = 1, GroupId = 1, IsApproved = false, Group = group };

            _context.Users.Add(manager);
            _context.Groups.Add(group);
            _context.ProjectGroups.Add(projectGroup);
            await _context.SaveChangesAsync();

            _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
                            .ReturnsAsync(manager);

            var result = await _controller.ApproveGroupAddition(1, 1);
            var json = result as JsonResult;

            Assert.That(json, Is.Not.Null);

            var serialized = JsonConvert.SerializeObject(json.Value);
            var dict = JsonConvert.DeserializeObject<Dictionary<string, object>>(serialized);

            Assert.That(dict["success"], Is.EqualTo(true));
            Assert.That(dict["message"].ToString(), Is.EqualTo("Group approved successfully."));
        }

    }
}
