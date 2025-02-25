using NUnit.Framework;
using TicketSystemWeb.Models;
using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace TicketSystemWeb.Tests.ViewModels
{
    [TestFixture]
    public class LoginViewModelTests
    {
        [Test]
        public void LoginViewModel_DefaultValuesAreCorrect()
        {
            var viewModel = new LoginViewModel
            {
                Username = "TestUser",
                Password = "SecurePass123"
            };

            Assert.That(viewModel.Username, Is.EqualTo("TestUser"));
            Assert.That(viewModel.Password, Is.EqualTo("SecurePass123"));
        }

        [Test]
        public void LoginViewModel_CanGetAndSetProperties()
        {
            var viewModel = new LoginViewModel { Username = "TestUser", Password = "SecurePass" };

            var username = viewModel.Username;
            var password = viewModel.Password;

            Assert.That(username, Is.EqualTo("TestUser"));
            Assert.That(password, Is.EqualTo("SecurePass"));
        }

        [Test]
        public void LoginViewModel_UsernameIsRequired()
        {
            var model = new LoginViewModel { Username = "", Password = "ValidPassword123" };

            var results = ValidateModel(model);

            Assert.That(results.Count, Is.EqualTo(1), "Expected one validation error.");
            Assert.That(results[0].ErrorMessage, Is.EqualTo("Username is required."));
        }

        [Test]
        public void LoginViewModel_PasswordIsRequired()
        {
            var model = new LoginViewModel { Username = "ValidUsername", Password = ""};

            var results = ValidateModel(model);

            Assert.That(results.Count, Is.EqualTo(1), "Expected one validation error.");
            Assert.That(results[0].ErrorMessage, Is.EqualTo("Password is required."));
        }

        private static List<ValidationResult> ValidateModel(object model)
        {
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(model, context, results, true);
            return results;
        }
    }
}
