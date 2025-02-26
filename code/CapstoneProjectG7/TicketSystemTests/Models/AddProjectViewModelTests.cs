using NUnit.Framework;
using System.Collections.Generic;
using TicketSystemWeb.ViewModels;
using TicketSystemWeb.Models.ProjectManagement.Group;
using TicketSystemWeb.Models.Employee;

namespace TicketSystemWeb.Tests.ViewModels
{
    [TestFixture]
    public class AddProjectViewModelTests
    {
        [Test]
        public void AddProjectViewModel_DefaultValues_AreCorrect()
        {
            // Arrange
            var viewModel = new AddProjectViewModel();

            // Assert
            Assert.That(viewModel.Title, Is.EqualTo(string.Empty));
            Assert.That(viewModel.Description, Is.EqualTo(string.Empty));
            Assert.That(viewModel.GroupIds, Is.Empty);
            Assert.That(viewModel.AllGroups, Is.Empty);
            Assert.That(viewModel.AllEmployees, Is.Null);
        }

        [Test]
        public void AddProjectViewModel_CanGetAndSetProperties()
        {
            // Arrange
            var viewModel = new AddProjectViewModel
            {
                Title = "Project Alpha",
                Description = "This is a sample project.",
                GroupIds = new List<int> { 1, 2, 3 },
                AllGroups = new List<Group> { new Group { Id = 1, Name = "Group A" } },
                AllEmployees = new List<Employee>
                {
                    new Employee { Id = "101", UserName = "johndoe", Email = "johndoe@example.com" }
                }
            };

            // Act
            var title = viewModel.Title;
            var description = viewModel.Description;
            var groupIds = viewModel.GroupIds;
            var allGroups = viewModel.AllGroups;
            var allEmployees = viewModel.AllEmployees;

            // Assert
            Assert.That(title, Is.EqualTo("Project Alpha"));
            Assert.That(description, Is.EqualTo("This is a sample project."));
            Assert.That(groupIds.Count, Is.EqualTo(3));
            Assert.That(allGroups.Count, Is.EqualTo(1));
            Assert.That(allEmployees, Is.Not.Null);
            Assert.That(allEmployees.Count, Is.EqualTo(1));
            Assert.That(allEmployees[0].UserName, Is.EqualTo("johndoe"));
            Assert.That(allEmployees[0].Email, Is.EqualTo("johndoe@example.com"));
        }
    }
}
