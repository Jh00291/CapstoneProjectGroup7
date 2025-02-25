using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using NUnit.Framework;
using TicketSystemWeb.Controllers;
using TicketSystemWeb.Data;
using TicketSystemWeb.Models.Employee;
using TicketSystemWeb.Models.ProjectManagement.Project;
using TicketSystemWeb.ViewModels;

namespace TicketSystemWeb.Tests.Controllers
{
    [TestFixture]
    public class AddProjectControllerTests : IDisposable
    {
        private TicketDBContext _context;
        private Mock<UserManager<Employee>> _userManagerMock;
        private AddProjectController _controller;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<TicketDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _context = new TicketDBContext(options);

            _userManagerMock = new Mock<UserManager<Employee>>(
                Mock.Of<IUserStore<Employee>>(), null, null, null, null, null, null, null, null);

            _controller = new AddProjectController(_context, _userManagerMock.Object);
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

        public void Dispose()
        {
            _context?.Dispose();
            _controller?.Dispose();
        }

        [Test]
        public async Task Create_InvalidModel_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("Error", "Invalid data");

            var result = await _controller.Create(new AddProjectViewModel());

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task Create_ValidModel_ReturnsRedirect()
        {
            var user = new Employee { Id = "user1" };
            _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync(user);

            var model = new AddProjectViewModel
            {
                Title = "New Project",
                Description = "Project Description",
                GroupIds = new List<int> { 1, 2 }
            };

            var result = await _controller.Create(model);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var redirect = (RedirectToActionResult)result;
            Assert.That(redirect.ActionName, Is.EqualTo("Management"));
            Assert.That(redirect.ControllerName, Is.EqualTo("ProjectManagement"));
        }
    }
}
