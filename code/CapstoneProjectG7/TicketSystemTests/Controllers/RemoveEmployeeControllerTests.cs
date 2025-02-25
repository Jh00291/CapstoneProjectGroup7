using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using TicketSystemWeb.Controllers;
using TicketSystemWeb.Models.Employee;

namespace TicketSystemWeb.Tests.Controllers
{
    [TestFixture]
    public class RemoveEmployeeControllerTests : IDisposable
    {
        private Mock<UserManager<Employee>> _userManagerMock;
        private RemoveEmployeeController _controller;

        [SetUp]
        public void SetUp()
        {
            _userManagerMock = new Mock<UserManager<Employee>>(
                Mock.Of<IUserStore<Employee>>(), null, null, null, null, null, null, null, null);

            _controller = new RemoveEmployeeController(_userManagerMock.Object);
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
        public async Task RemoveEmployee_NullId_ReturnsBadRequest()
        {
            var result = await _controller.RemoveEmployee(null);
            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task RemoveEmployee_UserNotFound_ReturnsNotFound()
        {
            _userManagerMock.Setup(um => um.FindByIdAsync(It.IsAny<string>())).ReturnsAsync((Employee)null);
            var result = await _controller.RemoveEmployee("123");
            Assert.That(result, Is.TypeOf<NotFoundObjectResult>());
        }

        [Test]
        public async Task RemoveEmployee_ValidUser_DeletesAndRedirects()
        {
            var user = new Employee { Id = "123", UserName = "TestUser", Email = "test@example.com" };

            _userManagerMock.Setup(um => um.FindByIdAsync("123")).ReturnsAsync(user);
            _userManagerMock.Setup(um => um.DeleteAsync(user)).ReturnsAsync(IdentityResult.Success);

            var result = await _controller.RemoveEmployee("123") as RedirectToActionResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Employees"));

            _userManagerMock.Verify(u => u.DeleteAsync(user), Times.Once());
        }

        [Test]
        public async Task RemoveEmployee_DeleteFails_ReturnsRedirectWithError()
        {
            // Arrange
            var employee = new Employee { Id = "123", UserName = "TestUser", Email = "test@example.com" };
            _userManagerMock.Setup(u => u.FindByIdAsync("123")).ReturnsAsync(employee);
            _userManagerMock.Setup(u => u.DeleteAsync(employee)).ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error removing employee." }));

            // Act
            var result = await _controller.RemoveEmployee("123") as RedirectToActionResult;

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.ActionName, Is.EqualTo("Employees"));

            
            Assert.That(_controller.ModelState[""].Errors[0].ErrorMessage, Is.EqualTo("Error removing employee."));

            _userManagerMock.Verify(u => u.DeleteAsync(employee), Times.Once());
        }

    }
}
