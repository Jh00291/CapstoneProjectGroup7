using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Moq;
using NUnit.Framework;
using TicketSystemWeb.Controllers;
using TicketSystemWeb.Models;
using TicketSystemWeb.Models.Employee;

namespace TicketSystemWeb.Tests.Controllers
{
    [TestFixture]
    public class AccountControllerTests : IDisposable
    {
        private Mock<SignInManager<Employee>> _signInManagerMock;
        private Mock<UserManager<Employee>> _userManagerMock;
        private AccountController _controller;

        [SetUp]
        public void SetUp()
        {
            var userStoreMock = new Mock<IUserStore<Employee>>();
            _userManagerMock = new Mock<UserManager<Employee>>(
                userStoreMock.Object, null, null, null, null, null, null, null, null);

            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var userClaimsFactoryMock = new Mock<IUserClaimsPrincipalFactory<Employee>>();

            _signInManagerMock = new Mock<SignInManager<Employee>>(
                _userManagerMock.Object,
                httpContextAccessorMock.Object,
                userClaimsFactoryMock.Object,
                null, null, null, null);

            _controller = new AccountController(_signInManagerMock.Object, _userManagerMock.Object);
        }


        [TearDown]
        public void TearDown()
        {
            _controller?.Dispose();
        }

        [OneTimeTearDown]
        public void OneTimeTearDown()
        {
            Dispose();
        }

        public void Dispose()
        {
            _controller?.Dispose();
        }

        [Test]
        public void Login_ReturnsView()
        {
            var result = _controller.Login();
            Assert.That(result, Is.TypeOf<ViewResult>());
        }

        [Test]
        public async Task Login_ValidCredentials_RedirectsToHome()
        {
            var model = new LoginViewModel { Username = "testuser", Password = "password123" };

            _signInManagerMock
                .Setup(s => s.PasswordSignInAsync(model.Username, model.Password, false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Success);

            var result = await _controller.Login(model);

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var redirect = (RedirectToActionResult)result;
            Assert.That(redirect.ActionName, Is.EqualTo("Index"));
            Assert.That(redirect.ControllerName, Is.EqualTo("Home"));
        }

        [Test]
        public async Task Login_InvalidCredentials_ReturnsViewWithError()
        {
            var model = new LoginViewModel { Username = "testuser", Password = "wrongpassword" };

            _signInManagerMock
                .Setup(s => s.PasswordSignInAsync(model.Username, model.Password, false, false))
                .ReturnsAsync(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            var result = await _controller.Login(model);

            Assert.That(result, Is.TypeOf<ViewResult>());
            Assert.That(_controller.ModelState[""].Errors.Count, Is.EqualTo(1));
        }

        [Test]
        public async Task Logout_RedirectsToLogin()
        {
            _signInManagerMock.Setup(s => s.SignOutAsync()).Returns(Task.CompletedTask);

            var result = await _controller.Logout();

            Assert.That(result, Is.TypeOf<RedirectToActionResult>());
            var redirect = (RedirectToActionResult)result;
            Assert.That(redirect.ActionName, Is.EqualTo("Login"));
            Assert.That(redirect.ControllerName, Is.EqualTo("Account"));
        }
    }
}
