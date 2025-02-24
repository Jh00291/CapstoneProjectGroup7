using NUnit.Framework;
using System.Collections.Generic;
using TicketSystemWeb.ViewModels;
using TicketSystemWeb.Models.Employee;

namespace TicketSystemWeb.Tests.ViewModels
{
    [TestFixture]
    public class EditGroupViewModelTests
    {
        [Test]
        public void EditGroupViewModel_DefaultValuesAreCorrect()
        {
            // Arrange
            var viewModel = new EditGroupViewModel();

            // Assert
            Assert.That(viewModel.GroupId, Is.EqualTo(0));
            Assert.That(viewModel.Name, Is.EqualTo(string.Empty));
            Assert.That(viewModel.SelectedManagerId, Is.Null);
            Assert.That(viewModel.AllEmployees, Is.Empty);
            Assert.That(viewModel.SelectedEmployeeIds, Is.Empty);
        }

        [Test]
        public void EditGroupViewModel_CanGetAndSetProperties()
        {
            // Arrange
            var viewModel = new EditGroupViewModel
            {
                GroupId = 10,
                Name = "Dev Team",
                SelectedManagerId = "manager789",
                AllEmployees = new List<Employee> { new Employee { Id = "emp5" } },
                SelectedEmployeeIds = new List<string> { "emp5" }
            };

            // Act (Explicitly get each property)
            var groupId = viewModel.GroupId;
            var name = viewModel.Name;
            var managerId = viewModel.SelectedManagerId;
            var employees = viewModel.AllEmployees;
            var selectedIds = viewModel.SelectedEmployeeIds;

            // Assert
            Assert.That(groupId, Is.EqualTo(10));
            Assert.That(name, Is.EqualTo("Dev Team"));
            Assert.That(managerId, Is.EqualTo("manager789"));
            Assert.That(employees.Count, Is.EqualTo(1));
            Assert.That(selectedIds.Count, Is.EqualTo(1));
        }


        [Test]
        public void EditGroupViewModel_CanSetValues()
        {
            // Arrange
            var viewModel = new EditGroupViewModel
            {
                GroupId = 10,
                Name = "QA Team",
                SelectedManagerId = "manager456",
                AllEmployees = new List<Employee> { new Employee { Id = "emp3" }, new Employee { Id = "emp4" } },
                SelectedEmployeeIds = new List<string> { "emp3", "emp4" }
            };

            // Assert
            Assert.That(viewModel.GroupId, Is.EqualTo(10));
            Assert.That(viewModel.Name, Is.EqualTo("QA Team"));
            Assert.That(viewModel.SelectedManagerId, Is.EqualTo("manager456"));
            Assert.That(viewModel.AllEmployees.Count, Is.EqualTo(2));
            Assert.That(viewModel.SelectedEmployeeIds.Count, Is.EqualTo(2));
        }
    }
}
