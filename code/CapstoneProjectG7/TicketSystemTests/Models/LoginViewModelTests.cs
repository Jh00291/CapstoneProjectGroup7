using NUnit.Framework;
using TicketSystemWeb.Models;

namespace TicketSystemWeb.Tests.ViewModels
{
    [TestFixture]
    public class LoginViewModelTests
    {
        [Test]
        public void LoginViewModel_DefaultValuesAreCorrect()
        {
            // Arrange
            var viewModel = new LoginViewModel
            {
                Username = "TestUser",
                Password = "SecurePass123"
            };

            // Assert
            Assert.That(viewModel.Username, Is.EqualTo("TestUser"));
            Assert.That(viewModel.Password, Is.EqualTo("SecurePass123"));
        }

        [Test]
        public void LoginViewModel_CanGetAndSetProperties()
        {
            // Arrange
            var viewModel = new LoginViewModel { Username = "TestUser", Password = "SecurePass" };

            // Act (Explicitly get each property)
            var username = viewModel.Username;
            var password = viewModel.Password;

            // Assert
            Assert.That(username, Is.EqualTo("TestUser"));
            Assert.That(password, Is.EqualTo("SecurePass"));
        }

    }
}
