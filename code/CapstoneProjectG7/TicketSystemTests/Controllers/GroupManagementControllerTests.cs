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
using TicketSystemWeb.Models.ProjectManagement.Group;
using TicketSystemWeb.ViewModels;

namespace TicketSystemWeb.Tests.Controllers
{
    [TestFixture]
    public class GroupManagementControllerTests : IDisposable
    {
        private TicketDBContext _context;
        private Mock<UserManager<Employee>> _userManagerMock;
        private GroupManagementController _controller;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<TicketDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _context = new TicketDBContext(options);

            _userManagerMock = new Mock<UserManager<Employee>>(
                Mock.Of<IUserStore<Employee>>(), null, null, null, null, null, null, null, null);

            _controller = new GroupManagementController(_context, _userManagerMock.Object);
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
        public async Task CreateGroup_InvalidModel_ReturnsBadRequest()
        {
            _controller.ModelState.AddModelError("Error", "Invalid data");

            var result = await _controller.CreateGroup(new AddGroupViewModel());

            Assert.That(result, Is.TypeOf<BadRequestObjectResult>());
        }

        [Test]
        public async Task CreateGroup_InvalidManager_ReturnsManagementView()
        {
            var model = new AddGroupViewModel
            {
                Name = "Test Group",
                SelectedManagerId = "invalid_manager_id",
                SelectedEmployeeIds = new List<string> { "emp1", "emp2" }
            };

            var result = await _controller.CreateGroup(model) as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.ViewName, Is.EqualTo("Management"));
        }

        [Test]
        public async Task CreateGroup_ValidModel_ReturnsRedirectToManagement()
        {
            var manager = new Employee { Id = "manager1" };
            _context.Users.Add(manager);
            await _context.SaveChangesAsync();

            var model = new AddGroupViewModel
            {
                Name = "Valid Group",
                SelectedManagerId = manager.Id,
                SelectedEmployeeIds = new List<string>()
            };

            var result = await _controller.CreateGroup(model);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var redirect = (RedirectToActionResult)result;
            Assert.That(redirect.ActionName, Is.EqualTo("Management"));
            Assert.That(redirect.ControllerName, Is.EqualTo("ProjectManagement"));
        }
    }
}
