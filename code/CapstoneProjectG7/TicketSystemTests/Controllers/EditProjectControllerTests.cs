using System;
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
    public class EditProjectControllerTests : IDisposable
    {
        private TicketDBContext _context;
        private Mock<UserManager<Employee>> _userManagerMock;
        private EditProjectController _controller;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<TicketDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _context = new TicketDBContext(options);

            _userManagerMock = new Mock<UserManager<Employee>>(
                Mock.Of<IUserStore<Employee>>(), null, null, null, null, null, null, null, null);

            _controller = new EditProjectController(_context, _userManagerMock.Object);
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
        public async Task UpdateProject_ProjectNotFound_ReturnsNotFound()
        {
            var model = new EditProjectViewModel { ProjectId = 1 };
            var result = await _controller.UpdateProject(model);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task UpdateProject_InvalidModel_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("Error", "Invalid data");
            var result = await _controller.UpdateProject(new EditProjectViewModel());

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task UpdateProject_AuthorizedUser_UpdatesProject()
        {
            var user = new Employee { Id = "manager1" };
            _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync(user);

            var project = new Project { Id = 1, Title = "Old Title", ProjectManagerId = "manager1" };
            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            var model = new EditProjectViewModel { ProjectId = 1, Title = "Updated Title" };
            var result = await _controller.UpdateProject(model);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
        }
    }
}
