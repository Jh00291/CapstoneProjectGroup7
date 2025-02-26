using NUnit.Framework;
using System.Collections.Generic;
using TicketSystemWeb.ViewModels;
using TicketSystemWeb.Models.Employee;
using TicketSystemWeb.Models.ProjectManagement.Group;
using TicketSystemWeb.Models.ProjectManagement.Project;

namespace TicketSystemWeb.Tests.ViewModels
{
    [TestFixture]
    public class ManagementViewModelTests
    {
        [Test]
        public void ManagementViewModel_DefaultValues_AreCorrect()
        {
            // Arrange
            var viewModel = new ManagementViewModel();

            // Assert
            Assert.That(viewModel.Projects, Is.Empty);
            Assert.That(viewModel.Groups, Is.Empty);
            Assert.That(viewModel.AllEmployees, Is.Empty);
            Assert.That(viewModel.CanAddProject, Is.False);
            Assert.That(viewModel.CanManageGroups, Is.False);
        }

        [Test]
        public void ManagementViewModel_CanAssignValues()
        {
            // Arrange
            var viewModel = new ManagementViewModel
            {
                Projects = new List<Project> { new Project { Id = 1, Title = "Project 1" } },
                Groups = new List<Group> { new Group { Id = 2, Name = "Team 2" } },
                AllEmployees = new List<Employee> { new Employee { Id = "emp1" } },
                CanAddProject = true,
                CanManageGroups = true
            };

            // Act (Explicitly get each property)
            var projects = viewModel.Projects;
            var groups = viewModel.Groups;
            var employees = viewModel.AllEmployees;
            var canAddProject = viewModel.CanAddProject;
            var canManageGroups = viewModel.CanManageGroups;

            // Assert
            Assert.That(projects.Count, Is.EqualTo(1));
            Assert.That(groups.Count, Is.EqualTo(1));
            Assert.That(employees.Count, Is.EqualTo(1));
            Assert.That(canAddProject, Is.True);
            Assert.That(canManageGroups, Is.True);
        }

    }
}
