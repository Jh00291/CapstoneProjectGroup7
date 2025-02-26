using NUnit.Framework;
using TicketSystemWeb.ViewModels;

namespace TicketSystemWeb.Tests.ViewModels
{
    [TestFixture]
    public class EditEmployeeViewModelTests
    {
        [Test]
        public void EditEmployeeViewModel_DefaultValuesAreCorrect()
        {
            // Arrange & Act
            var viewModel = new EditEmployeeViewModel();

            // Assert
            Assert.That(viewModel.UserName, Is.EqualTo(string.Empty));
            Assert.That(viewModel.Email, Is.EqualTo(string.Empty));
            Assert.That(viewModel.Role, Is.EqualTo("user"));
        }

        [Test]
        public void EditEmployeeViewModel_CanSetProperties()
        {
            // Arrange
            var viewModel = new EditEmployeeViewModel
            {
                Id = "123",
                UserName = "UpdatedUser",
                Email = "updated@example.com",
                Role = "manager"
            };

            // Assert
            Assert.That(viewModel.Id, Is.EqualTo("123"));
            Assert.That(viewModel.UserName, Is.EqualTo("UpdatedUser"));
            Assert.That(viewModel.Email, Is.EqualTo("updated@example.com"));
            Assert.That(viewModel.Role, Is.EqualTo("manager"));
        }
    }
}
