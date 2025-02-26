using NUnit.Framework;
using System.Collections.Generic;
using TicketSystemWeb.ViewModels;
using TicketSystemWeb.Models.Employee;

namespace TicketSystemWeb.Tests.ViewModels
{
    [TestFixture]
    public class AddGroupViewModelTests
    {
        [Test]
        public void AddGroupViewModel_DefaultValuesAreCorrect()
        {
            // Arrange
            var viewModel = new AddGroupViewModel();

            // Assert
            Assert.That(viewModel.Name, Is.EqualTo(string.Empty));
            Assert.That(viewModel.SelectedManagerId, Is.Null);
            Assert.That(viewModel.AllEmployees, Is.Empty);
            Assert.That(viewModel.SelectedEmployeeIds, Is.Empty);
        }

        [Test]
        public void AddGroupViewModel_CanGetAndSetProperties()
        {
            // Arrange
            var viewModel = new AddGroupViewModel
            {
                Name = "QA Team",
                SelectedManagerId = "manager456",
                AllEmployees = new List<Employee> { new Employee { Id = "emp3" } },
                SelectedEmployeeIds = new List<string> { "emp3" }
            };

            // Act (Explicitly get each property)
            var name = viewModel.Name;
            var managerId = viewModel.SelectedManagerId;
            var employees = viewModel.AllEmployees;
            var selectedIds = viewModel.SelectedEmployeeIds;

            // Assert
            Assert.That(name, Is.EqualTo("QA Team"));
            Assert.That(managerId, Is.EqualTo("manager456"));
            Assert.That(employees.Count, Is.EqualTo(1));
            Assert.That(selectedIds.Count, Is.EqualTo(1));
        }


        [Test]
        public void AddGroupViewModel_CanSetValues()
        {
            // Arrange
            var viewModel = new AddGroupViewModel
            {
                Name = "Development Team",
                SelectedManagerId = "manager123",
                AllEmployees = new List<Employee> { new Employee { Id = "emp1" }, new Employee { Id = "emp2" } },
                SelectedEmployeeIds = new List<string> { "emp1", "emp2" }
            };

            // Assert
            Assert.That(viewModel.Name, Is.EqualTo("Development Team"));
            Assert.That(viewModel.SelectedManagerId, Is.EqualTo("manager123"));
            Assert.That(viewModel.AllEmployees.Count, Is.EqualTo(2));
            Assert.That(viewModel.SelectedEmployeeIds.Count, Is.EqualTo(2));
        }
    }
}
