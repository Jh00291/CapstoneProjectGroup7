using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using TicketSystemWeb.Controllers;
using TicketSystemWeb.Models.Employee;
using TicketSystemWeb.ViewModels;

namespace TicketSystemWeb.Tests.Controllers
{
    [TestFixture]
    public class EditEmployeeControllerTests : IDisposable
    {
        private Mock<UserManager<Employee>> _userManagerMock;
        private EditEmployeeController _controller;

        [SetUp]
        public void SetUp()
        {
            _userManagerMock = new Mock<UserManager<Employee>>(
                Mock.Of<IUserStore<Employee>>(), null, null, null, null, null, null, null, null);

            _controller = new EditEmployeeController(_userManagerMock.Object);
            _controller.ModelState.Clear();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            _controller?.Dispose();
        }

        public void Dispose()
        {
            _controller?.Dispose();
        }

        [Test]
        public async Task EditEmployee_NullId_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.EditEmployee(null);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.That(badRequestResult.Value, Is.EqualTo("Invalid employee ID."));
        }

        [Test]
        public async Task EditEmployee_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((Employee)null);

            // Act
            var result = await _controller.EditEmployee("123");

            // Assert
            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
            var notFoundResult = (NotFoundObjectResult)result;
            Assert.That(notFoundResult.Value, Is.EqualTo("Employee not found."));
        }

        [Test]
        public async Task EditEmployee_ValidId_ReturnsPartialView()
        {
            // Arrange
            var employee = new Employee { Id = "123", UserName = "TestUser", Email = "test@example.com" };
            _userManagerMock.Setup(u => u.FindByIdAsync("123")).ReturnsAsync(employee);
            _userManagerMock.Setup(u => u.GetRolesAsync(employee)).ReturnsAsync(new List<string> { "Admin" });

            // Act
            var result = await _controller.EditEmployee("123");

            // Assert
            Assert.That(result, Is.TypeOf<PartialViewResult>());
            var viewResult = (PartialViewResult)result;
            Assert.That(viewResult.ViewName, Is.EqualTo("EditEmployee"));
            Assert.That(viewResult.Model, Is.TypeOf<EditEmployeeViewModel>());

            var model = (EditEmployeeViewModel)viewResult.Model;
            Assert.That(model.Id, Is.EqualTo("123"));
            Assert.That(model.UserName, Is.EqualTo("TestUser"));
            Assert.That(model.Email, Is.EqualTo("test@example.com"));
            Assert.That(model.Role, Is.EqualTo("Admin"));
        }

        [Test]
        public async Task UpdateEmployee_InvalidModel_ReturnsBadRequest()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Invalid data.");

            var model = new EditEmployeeViewModel { Id = "123", UserName = "UpdatedUser", Email = "updated@example.com", Role = "Admin" };

            // Act
            var result = await _controller.UpdateEmployee(model);

            // Assert
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
            var badRequestResult = (BadRequestObjectResult)result;
            Assert.That(badRequestResult.Value, Is.EqualTo("Invalid data."));
        }

        [Test]
        public async Task UpdateEmployee_UserNotFound_ReturnsNotFound()
        {
            // Arrange
            _userManagerMock.Setup(u => u.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((Employee)null);
            var model = new EditEmployeeViewModel { Id = "123", UserName = "UpdatedUser", Email = "updated@example.com", Role = "Admin" };

            // Act
            var result = await _controller.UpdateEmployee(model);

            // Assert
            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
            var notFoundResult = (NotFoundObjectResult)result;
            Assert.That(notFoundResult.Value, Is.EqualTo("Employee not found."));
        }

        [Test]
        public async Task UpdateEmployee_ValidModel_UpdatesUserAndRedirects()
        {
            // Arrange
            var employee = new Employee { Id = "123", UserName = "OldUser", Email = "old@example.com" };
            _userManagerMock.Setup(u => u.FindByIdAsync("123")).ReturnsAsync(employee);
            _userManagerMock.Setup(u => u.GetRolesAsync(employee)).ReturnsAsync(new List<string> { "User" });
            _userManagerMock.Setup(u => u.RemoveFromRolesAsync(employee, It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.AddToRoleAsync(employee, "Admin")).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.UpdateAsync(employee)).ReturnsAsync(IdentityResult.Success);

            var model = new EditEmployeeViewModel { Id = "123", UserName = "UpdatedUser", Email = "updated@example.com", Role = "Admin" };

            // Act
            var result = await _controller.UpdateEmployee(model);

            // Assert
            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var redirectResult = (RedirectToActionResult)result;
            Assert.That(redirectResult.ActionName, Is.EqualTo("Employees"));
            Assert.That(redirectResult.ControllerName, Is.EqualTo("Employees"));

            // Verify User Updates
            Assert.That(employee.UserName, Is.EqualTo("UpdatedUser"));
            Assert.That(employee.Email, Is.EqualTo("updated@example.com"));

            _userManagerMock.Verify(u => u.UpdateAsync(employee), Times.Once());
        }

        [Test]
        public async Task UpdateEmployee_UpdateFails_ReturnsPartialViewWithError()
        {
            // Arrange
            var employee = new Employee { Id = "123", UserName = "OldUser", Email = "old@example.com" };
            _userManagerMock.Setup(u => u.FindByIdAsync("123")).ReturnsAsync(employee);
            _userManagerMock.Setup(u => u.GetRolesAsync(employee)).ReturnsAsync(new List<string> { "User" });
            _userManagerMock.Setup(u => u.RemoveFromRolesAsync(employee, It.IsAny<IEnumerable<string>>())).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.AddToRoleAsync(employee, "Admin")).ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(u => u.UpdateAsync(employee)).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error updating employee." }));

            var model = new EditEmployeeViewModel { Id = "123", UserName = "UpdatedUser", Email = "updated@example.com", Role = "Admin" };

            // Act
            var result = await _controller.UpdateEmployee(model);

            // Assert
            Assert.That(result, Is.TypeOf<PartialViewResult>());
            var viewResult = (PartialViewResult)result;
            Assert.That(viewResult.ViewName, Is.EqualTo("EditEmployee"));
            Assert.That(viewResult.Model, Is.EqualTo(model));

            // Ensure Error Message is in ModelState
            Assert.That(_controller.ModelState[""].Errors[0].ErrorMessage, Is.EqualTo("Error updating employee."));

            _userManagerMock.Verify(u => u.UpdateAsync(employee), Times.Once());
        }

        [Test]
        public async Task EditEmployee_UserHasNoRoles_AssignsDefaultRole()
        {
            // Arrange
            var employee = new Employee { Id = "123", UserName = "TestUser", Email = "test@example.com" };
            _userManagerMock.Setup(u => u.FindByIdAsync("123")).ReturnsAsync(employee);
            _userManagerMock.Setup(u => u.GetRolesAsync(employee)).ReturnsAsync(new List<string>()); // Empty roles

            // Act
            var result = await _controller.EditEmployee("123") as PartialViewResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.EqualTo("EditEmployee"));

            var model = result.Model as EditEmployeeViewModel;
            Assert.That(model, Is.Not.Null);
            Assert.That(model.Role, Is.EqualTo("user")); // Default role should be assigned
        }

    }
}
