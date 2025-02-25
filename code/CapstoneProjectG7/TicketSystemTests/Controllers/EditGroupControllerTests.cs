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
using TicketSystemWeb.ViewModels;

namespace TicketSystemWeb.Tests.Controllers
{
    [TestFixture]
    public class EditGroupControllerTests : IDisposable
    {
        private TicketDBContext _context;
        private Mock<UserManager<Employee>> _userManagerMock;
        private EditGroupController _editGroupController;

        [SetUp]
        public void SetUp()
        {
            var options = new DbContextOptionsBuilder<TicketDBContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;
            _context = new TicketDBContext(options);

            _userManagerMock = new Mock<UserManager<Employee>>(
                Mock.Of<IUserStore<Employee>>(), null, null, null, null, null, null, null, null);

            _editGroupController = new EditGroupController(_context, _userManagerMock.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Database.EnsureDeleted(); // Clear data between tests
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Dispose();
        }

        public void Dispose()
        {
            _context?.Dispose();
            _editGroupController?.Dispose();
        }

        [Test]
        public async Task EditGroup_GroupNotFound_ReturnsNotFound()
        {
            var result = await _editGroupController.Edit(1);
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task UpdateGroup_GroupNotFound_ReturnsNotFound()
        {
            var result = await _editGroupController.UpdateGroup(new EditGroupViewModel { GroupId = 1 });
            Assert.That(result, Is.TypeOf<NotFoundResult>());
        }

        [Test]
        public async Task UpdateGroup_UnauthorizedUser_ReturnsForbid()
        {
            var group = new Group { Id = 1, ManagerId = "123" };
            _context.Groups.Add(group);
            await _context.SaveChangesAsync();

            _userManagerMock.Setup(u => u.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>())).Returns("456");
            _editGroupController.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = new System.Security.Claims.ClaimsPrincipal() }
            };

            var result = await _editGroupController.UpdateGroup(new EditGroupViewModel { GroupId = 1 });
            Assert.That(result, Is.TypeOf<ForbidResult>());
        }
    }
}
