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
using TicketSystemWeb.Models.ProjectManagement.Group;

namespace TicketSystemWeb.Tests.Controllers
{
    [TestFixture]
    public class RemoveGroupControllerTests : IDisposable
    {
        private TicketDBContext _context;
        private Mock<UserManager<Employee>> _userManagerMock;
        private RemoveGroupController _controller;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<TicketDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _context = new TicketDBContext(options);

            _userManagerMock = new Mock<UserManager<Employee>>(
                Mock.Of<IUserStore<Employee>>(), null, null, null, null, null, null, null, null);

            _controller = new RemoveGroupController(_context, _userManagerMock.Object);
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
        public async Task DeleteGroup_GroupNotFound_ReturnsNotFound()
        {
            var result = await _controller.DeleteGroup(1);

            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task DeleteGroup_UnauthorizedUser_ReturnsForbid()
        {
            var group = new Group { Id = 1, ManagerId = "manager1" };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            var employee = new Employee { Id = "user1" };
            _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync(employee);
            _userManagerMock.Setup(u => u.IsInRoleAsync(employee, "admin")).ReturnsAsync(false);

            var result = await _controller.DeleteGroup(1);

            Assert.That(result, Is.TypeOf<ForbidResult>());
        }

        [Test]
        public async Task DeleteGroup_AdminUser_DeletesGroupAndRedirects()
        {
            var group = new Group { Id = 1, ManagerId = "manager1" };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            var adminUser = new Employee { Id = "adminUser" };
            _userManagerMock.Setup(u => u.GetUserAsync(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).ReturnsAsync(adminUser);
            _userManagerMock.Setup(u => u.IsInRoleAsync(adminUser, "admin")).ReturnsAsync(true);

            var result = await _controller.DeleteGroup(1);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var redirect = (RedirectToActionResult)result;
            Assert.That(redirect.ActionName, Is.EqualTo("Management"));
            Assert.That(redirect.ControllerName, Is.EqualTo("ProjectManagement"));
        }
    }
}
