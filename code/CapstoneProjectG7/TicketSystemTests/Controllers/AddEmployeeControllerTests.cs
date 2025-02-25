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
    public class AddEmployeeControllerTests : IDisposable
    {
        private Mock<UserManager<Employee>> _userManagerMock;
        private AddEmployeeController _controller;

        [SetUp]
        public void SetUp()
        {
            _userManagerMock = new Mock<UserManager<Employee>>(
                Mock.Of<IUserStore<Employee>>(), null, null, null, null, null, null, null, null);

            _controller = new AddEmployeeController(_userManagerMock.Object);

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
        public async Task AddEmployee_ValidModel_CreatesUserAndRedirects()
        {
            // Arrange
            _userManagerMock
                .Setup(u => u.CreateAsync(It.IsAny<Employee>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);

            _userManagerMock
                .Setup(u => u.AddToRoleAsync(It.IsAny<Employee>(), It.IsAny<string>()))
                .Returns(Task.FromResult(IdentityResult.Success));

            _controller.ModelState.Clear();

            // Act
            var result = await _controller.AddEmployee("TestUser", "test@example.com", "Password123!", "Admin");

            // Assert
            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var redirectResult = (RedirectToActionResult)result;
            Assert.That(redirectResult.ActionName, Is.EqualTo("Employees"));
            Assert.That(redirectResult.ControllerName, Is.EqualTo("Employees"));

            
            _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<Employee>(), It.IsAny<string>()), Times.Once());
            _userManagerMock.Verify(u => u.AddToRoleAsync(It.IsAny<Employee>(), "Admin"), Times.Once());
        }

        [Test]
        public async Task AddEmployee_InvalidModel_ReturnsView()
        {
            // Arrange
            _controller.ModelState.AddModelError("Error", "Invalid model");

            // Act
            var result = await _controller.AddEmployee("", "", "", "");

            // Assert
            Assert.That(result, Is.TypeOf<ViewResult>());

            _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<Employee>(), It.IsAny<string>()), Times.Never());
        }

        [Test]
        public async Task AddEmployee_UserCreationFails_ReturnsViewWithError()
        {
            // Arrange
            _userManagerMock
                .Setup(u => u.CreateAsync(It.IsAny<Employee>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Error creating user." }));

            // Act
            var result = await _controller.AddEmployee("TestUser", "test@example.com", "Password123!", "Admin");

            // Assert
            Assert.That(result, Is.TypeOf<ViewResult>());

            
            Assert.That(_controller.ModelState[""].Errors[0].ErrorMessage, Is.EqualTo("Error creating user."));

            
            _userManagerMock.Verify(u => u.CreateAsync(It.IsAny<Employee>(), It.IsAny<string>()), Times.Once());
        }
    }
}
