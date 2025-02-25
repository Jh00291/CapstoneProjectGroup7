using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using TicketSystemWeb.Controllers;
using TicketSystemWeb.Data;
using TicketSystemWeb.Models.Employee;

namespace TicketSystemWeb.Tests.Controllers
{
    [TestFixture]
    public class EmployeesControllerTests : IDisposable
    {
        private Mock<ILogger<EmployeesController>> _loggerMock;
        private Mock<UserManager<Employee>> _userManagerMock;
        private Mock<TicketDBContext> _dbContextMock;
        private EmployeesController _controller;

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<ILogger<EmployeesController>>();

            var users = new List<Employee>
            {
                new Employee { Id = "1", UserName = "User1", Email = "user1@example.com" },
                new Employee { Id = "2", UserName = "User2", Email = "user2@example.com" }
            }.AsQueryable();

            var mockUsersDbSet = new Mock<DbSet<Employee>>();
            mockUsersDbSet.As<IQueryable<Employee>>().Setup(m => m.Provider).Returns(users.Provider);
            mockUsersDbSet.As<IQueryable<Employee>>().Setup(m => m.Expression).Returns(users.Expression);
            mockUsersDbSet.As<IQueryable<Employee>>().Setup(m => m.ElementType).Returns(users.ElementType);
            mockUsersDbSet.As<IQueryable<Employee>>().Setup(m => m.GetEnumerator()).Returns(users.GetEnumerator());

            _dbContextMock = new Mock<TicketDBContext>(new DbContextOptions<TicketDBContext>());
            _dbContextMock.Setup(db => db.Users).Returns(mockUsersDbSet.Object);

            _userManagerMock = new Mock<UserManager<Employee>>(
                Mock.Of<IUserStore<Employee>>(), null, null, null, null, null, null, null, null);

            _controller = new EmployeesController(_loggerMock.Object, _dbContextMock.Object, _userManagerMock.Object);
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
        public async Task Employees_ReturnsViewWithUsers()
        {
            var result = await _controller.Employees() as ViewResult;

            Assert.That(result, Is.Not.Null);
            Assert.That(result.Model, Is.Not.Null);

            var model = result.Model as List<Employee>;
            Assert.That(model, Has.Count.EqualTo(2));
            Assert.That(model[0].UserName, Is.EqualTo("User1"));
            Assert.That(model[1].UserName, Is.EqualTo("User2"));

            _dbContextMock.Verify(db => db.Users, Times.Once());
        }
    }
}
