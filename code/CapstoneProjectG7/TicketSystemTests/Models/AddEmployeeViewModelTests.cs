using NUnit.Framework;
using TicketSystemWeb.ViewModels;

namespace TicketSystemWeb.Tests.ViewModels
{
    [TestFixture]
    public class AddEmployeeViewModelTests
    {
        [Test]
        public void AddEmployeeViewModel_DefaultValuesAreCorrect()
        {
            // Arrange & Act
            var viewModel = new AddEmployeeViewModel();

            // Assert
            Assert.That(viewModel.UserName, Is.EqualTo(string.Empty));
            Assert.That(viewModel.Email, Is.EqualTo(string.Empty));
            Assert.That(viewModel.Password, Is.EqualTo(string.Empty));
            Assert.That(viewModel.Role, Is.EqualTo("user"));
        }

        [Test]
        public void AddEmployeeViewModel_CanSetProperties()
        {
            // Arrange
            var viewModel = new AddEmployeeViewModel
            {
                UserName = "TestUser",
                Email = "test@example.com",
                Password = "SecurePass123",
                Role = "admin"
            };

            // Assert
            Assert.That(viewModel.UserName, Is.EqualTo("TestUser"));
            Assert.That(viewModel.Email, Is.EqualTo("test@example.com"));
            Assert.That(viewModel.Password, Is.EqualTo("SecurePass123"));
            Assert.That(viewModel.Role, Is.EqualTo("admin"));
        }
    }
}
