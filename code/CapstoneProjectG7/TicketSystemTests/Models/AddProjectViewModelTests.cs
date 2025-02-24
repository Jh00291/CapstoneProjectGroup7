using NUnit.Framework;
using System.Collections.Generic;
using TicketSystemWeb.ViewModels;
using TicketSystemWeb.Models.ProjectManagement.Group;

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
                AllGroups = new List<Group> { new Group { Id = 1, Name = "Group A" } }
            };

            // Act (Explicitly get each property)
            var title = viewModel.Title;
            var description = viewModel.Description;
            var groupIds = viewModel.GroupIds;
            var allGroups = viewModel.AllGroups;

            // Assert
            Assert.That(title, Is.EqualTo("Project Alpha"));
            Assert.That(description, Is.EqualTo("This is a sample project."));
            Assert.That(groupIds.Count, Is.EqualTo(3));
            Assert.That(allGroups.Count, Is.EqualTo(1));
        }

    }
}
