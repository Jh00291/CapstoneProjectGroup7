using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using Moq.EntityFrameworkCore;
using NUnit.Framework;
using TicketSystemWeb.Controllers;
using TicketSystemWeb.Data;
using TicketSystemWeb.Models.Employee;
using TicketSystemWeb.Models.ProjectManagement.Group;
using TicketSystemWeb.ViewModels;

namespace TicketSystemWeb.Tests.Controllers
{
    [TestFixture]
    public class EmployeesControllerTests : IDisposable
    {
        private Mock<UserManager<Employee>> _userManagerMock;
        private Mock<TicketDBContext> _contextMock;
        private Mock<ILogger<EmployeesController>> _loggerMock;
        private EmployeesController _controller;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<EmployeesController>>();
            _contextMock = new Mock<TicketDBContext>(new DbContextOptions<TicketDBContext>());

            var userStoreMock = new Mock<IUserStore<Employee>>();
            _userManagerMock = new Mock<UserManager<Employee>>(
                userStoreMock.Object, null!, null!, null!, null!, null!, null!, null!, null!);

            _controller = new EmployeesController(_loggerMock.Object, _contextMock.Object, _userManagerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        public void Dispose()
        {
            _controller?.Dispose();
        }

        [Test]
        public async Task Employees_ReturnsViewWithEmployeesList()
        {
            var employees = new List<Employee>
            {
                new Employee { Id = "1", UserName = "user1" },
                new Employee { Id = "2", UserName = "user2" }
            };
            _contextMock.Setup(c => c.Users).ReturnsDbSet(employees);
            _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<ClaimsPrincipal>())).ReturnsAsync(new Employee { Id = "1" });
            _userManagerMock.Setup(u => u.GetRolesAsync(It.IsAny<Employee>())).ReturnsAsync(new List<string> { "admin" });

            var result = await _controller.Employees();
            var viewResult = result as ViewResult;

            Assert.That(viewResult, Is.Not.Null);
            Assert.That(viewResult.Model, Is.TypeOf<Tuple<List<Employee>, AddEmployeeViewModel>>());
        }

        [Test]
        public async Task RemoveEmployee_UserHasManagedGroups_ReturnsBadRequest()
        {
            _userManagerMock.Setup(u => u.FindByIdAsync("1")).ReturnsAsync(new Employee { Id = "1", UserName = "testuser" });
            var groups = new List<Group> { new Group { ManagerId = "1", Name = "Group1" } };
            _contextMock.Setup(c => c.Groups).ReturnsDbSet(groups);

            var result = await _controller.RemoveEmployee("1");
            var badRequestResult = result as BadRequestObjectResult;

            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult.Value, Is.EqualTo("Please remove this user from Group1 before removing."));
        }

        [Test]
        public async Task RemoveEmployee_DeletionFails_ReturnsBadRequest()
        {
            var employee = new Employee { Id = "1", UserName = "testuser" };
            _userManagerMock.Setup(u => u.FindByIdAsync("1")).ReturnsAsync(employee);
            var groups = new List<Group>();
            _contextMock.Setup(c => c.Groups).ReturnsDbSet(groups);
            _userManagerMock.Setup(u => u.DeleteAsync(employee)).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Deletion failed" }));

            var result = await _controller.RemoveEmployee("1");
            var badRequestResult = result as BadRequestObjectResult;

            Assert.That(badRequestResult, Is.Not.Null);
            Assert.That(badRequestResult.Value, Is.EqualTo("Failed to remove the employee."));
        }

        [Test]
        public async Task AddEmployee_ExistingUser_ReturnsViewWithError()
        {
            var model = new AddEmployeeViewModel { UserName = "existinguser", Email = "test@example.com", Password = "password123", Role = "User" };
            var existingUser = new Employee { Id = "1", UserName = "existinguser" };
            _userManagerMock.Setup(u => u.FindByNameAsync(model.UserName)).ReturnsAsync(existingUser);
            _contextMock.Setup(c => c.Users).ReturnsDbSet(new List<Employee> { existingUser });

            var result = await _controller.AddEmployee(model);

            Assert.That(result, Is.TypeOf<ViewResult>());
        }

        [Test]
        public async Task EditEmployee_InvalidId_ReturnsBadRequest()
        {
            var result = await _controller.EditEmployee(null!);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task EditEmployee_NonExistentUser_ReturnsNotFound()
        {
            _userManagerMock.Setup(u => u.FindByIdAsync("123")).ReturnsAsync((Employee)null!);

            var result = await _controller.EditEmployee("123");

            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task RemoveEmployee_NonExistentUser_ReturnsNotFound()
        {
            _userManagerMock.Setup(u => u.FindByIdAsync("123")).ReturnsAsync((Employee)null!);

            var result = await _controller.RemoveEmployee("123");

            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task AddEmployee_UserCreationFails_ReturnsViewWithError()
        {
            var model = new AddEmployeeViewModel { UserName = "testuser", Email = "test@example.com", Password = "password123", Role = "User" };
            _userManagerMock.Setup(u => u.FindByNameAsync(model.UserName)).ReturnsAsync((Employee)null!);
            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<Employee>(), model.Password)).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Creation failed" }));

            _contextMock.Setup(c => c.Users).ReturnsDbSet(new List<Employee>());

            var result = await _controller.AddEmployee(model);

            Assert.That(result, Is.TypeOf<ViewResult>());
        }

        [Test]
        public async Task UpdateEmployee_InvalidModelState_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("Email", "Required");
            var model = new EditEmployeeViewModel { Id = "1", UserName = "testuser", Email = "", Role = "User" };

            var result = await _controller.UpdateEmployee(model);

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task UpdateEmployee_UpdateFails_ReturnsEditEmployeeView()
        {
            var employee = new Employee { Id = "1", UserName = "testuser", Email = "test@example.com" };
            _userManagerMock.Setup(u => u.FindByIdAsync(employee.Id)).ReturnsAsync(employee);
            _userManagerMock.Setup(u => u.UpdateAsync(employee)).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Update failed" }));
            _userManagerMock.Setup(u => u.GetRolesAsync(employee)).ReturnsAsync(new List<string> { "User" });

            var model = new EditEmployeeViewModel { Id = "1", UserName = "testuser", Email = "test@example.com", Role = "User" };
            var result = await _controller.UpdateEmployee(model);

            Assert.That(result, Is.TypeOf<PartialViewResult>());
        }

        [Test]
        public async Task AddEmployee_InvalidModelState_ReturnsViewWithEmployees()
        {
            var model = new AddEmployeeViewModel();
            _controller.ModelState.AddModelError("UserName", "Required");

            var employees = new List<Employee> { new Employee { Id = "1", UserName = "existinguser" } };
            _contextMock.Setup(c => c.Users).ReturnsDbSet(employees);

            var result = await _controller.AddEmployee(model);

            Assert.That(result, Is.TypeOf<ViewResult>());
        }

        [Test]
        public async Task EditEmployee_ValidUser_ReturnsPartialView()
        {
            var employee = new Employee { Id = "1", UserName = "testuser", Email = "test@example.com" };
            _userManagerMock.Setup(u => u.FindByIdAsync(employee.Id)).ReturnsAsync(employee);
            _userManagerMock.Setup(u => u.GetRolesAsync(employee)).ReturnsAsync(new List<string>());

            var result = await _controller.EditEmployee(employee.Id);

            Assert.That(result, Is.TypeOf<PartialViewResult>());
        }

        [Test]
        public async Task AddEmployee_UserCreationFailsWithMultipleErrors_ReturnsViewWithError()
        {
            var model = new AddEmployeeViewModel { UserName = "testuser", Email = "test@example.com", Password = "password123", Role = "User" };
            _userManagerMock.Setup(u => u.FindByNameAsync(model.UserName)).ReturnsAsync((Employee)null!);
            _userManagerMock.Setup(u => u.CreateAsync(It.IsAny<Employee>(), model.Password))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error1" }, new IdentityError { Description = "Error2" }));

            _contextMock.Setup(c => c.Users).ReturnsDbSet(new List<Employee>());

            var result = await _controller.AddEmployee(model);

            Assert.That(result, Is.TypeOf<ViewResult>());
            Assert.That(_controller.ViewData["ErrorMessage"], Does.Contain("Error1"));
            Assert.That(_controller.ViewData["ErrorMessage"], Does.Contain("Error2"));
        }

        [Test]
        public async Task UpdateEmployee_RoleChangeSuccessful_ReturnsRedirect()
        {
            var employee = new Employee { Id = "1", UserName = "testuser", Email = "test@example.com" };
            _userManagerMock.Setup(u => u.FindByIdAsync(employee.Id)).ReturnsAsync(employee);
            _userManagerMock.Setup(u => u.GetRolesAsync(employee)).ReturnsAsync(new List<string> { "OldRole" });
            _userManagerMock.Setup(u => u.RemoveFromRolesAsync(employee, It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.AddToRoleAsync(employee, "NewRole")).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.UpdateAsync(employee)).ReturnsAsync(IdentityResult.Success);

            var model = new EditEmployeeViewModel { Id = "1", UserName = "testuser", Email = "test@example.com", Role = "NewRole" };
            var result = await _controller.UpdateEmployee(model);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
        }

        [Test]
        public async Task UpdateEmployee_RoleChangeFails_ReturnsPartialViewWithErrors()
        {
            var employee = new Employee { Id = "1", UserName = "testuser", Email = "test@example.com" };
            _userManagerMock.Setup(u => u.FindByIdAsync(employee.Id)).ReturnsAsync(employee);
            _userManagerMock.Setup(u => u.GetRolesAsync(employee)).ReturnsAsync(new List<string> { "OldRole" });
            _userManagerMock.Setup(u => u.RemoveFromRolesAsync(employee, It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.AddToRoleAsync(employee, "NewRole")).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.UpdateAsync(employee)).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Update failed" }));

            var model = new EditEmployeeViewModel { Id = "1", UserName = "testuser", Email = "test@example.com", Role = "NewRole" };
            var result = await _controller.UpdateEmployee(model);

            Assert.That(result, Is.TypeOf<PartialViewResult>());
            Assert.That(_controller.ModelState[string.Empty].Errors, Is.Not.Empty);
        }

        [Test]
        public async Task RemoveEmployee_InvalidEmployeeId_ReturnsBadRequest()
        {
            var result = await _controller.RemoveEmployee("");

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task UpdateEmployee_NoRoleChange_RedirectsToEmployees()
        {
            var employee = new Employee { Id = "1", UserName = "testuser", Email = "test@example.com" };
            _userManagerMock.Setup(u => u.FindByIdAsync(employee.Id)).ReturnsAsync(employee);
            _userManagerMock.Setup(u => u.GetRolesAsync(employee)).ReturnsAsync(new List<string> { "User" });
            _userManagerMock.Setup(u => u.UpdateAsync(employee)).ReturnsAsync(IdentityResult.Success);

            var model = new EditEmployeeViewModel { Id = "1", UserName = "testuser", Email = "test@example.com", Role = "User" };
            var result = await _controller.UpdateEmployee(model);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
        }

    }
}