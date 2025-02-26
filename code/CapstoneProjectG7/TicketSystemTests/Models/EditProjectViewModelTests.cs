using NUnit.Framework;
using System.Collections.Generic;
using TicketSystemWeb.ViewModels;
using TicketSystemWeb.Models.ProjectManagement.Group;

namespace TicketSystemWeb.Tests.ViewModels
{
    [TestFixture]
    public class EditProjectViewModelTests
    {
        [Test]
        public void EditProjectViewModel_DefaultValues_AreCorrect()
        {
            // Arrange
            var viewModel = new EditProjectViewModel();

            // Assert
            Assert.That(viewModel.ProjectId, Is.EqualTo(0));
            Assert.That(viewModel.Title, Is.EqualTo(string.Empty));
            Assert.That(viewModel.Description, Is.EqualTo(string.Empty));
            Assert.That(viewModel.SelectedGroupIds, Is.Empty);
            Assert.That(viewModel.AllGroups, Is.Empty);
        }

        [Test]
        public void EditProjectViewModel_CanGetAndSetProperties()
        {
            // Arrange
            var viewModel = new EditProjectViewModel
            {
                ProjectId = 101,
                Title = "Project Beta",
                Description = "This is another sample project.",
                SelectedGroupIds = new List<int> { 1, 2 },
                AllGroups = new List<Group> { new Group { Id = 2, Name = "Group B" } }
            };

            // Act (Explicitly get each property)
            var projectId = viewModel.ProjectId;
            var title = viewModel.Title;
            var description = viewModel.Description;
            var selectedGroupIds = viewModel.SelectedGroupIds;
            var allGroups = viewModel.AllGroups;

            // Assert
            Assert.That(projectId, Is.EqualTo(101));
            Assert.That(title, Is.EqualTo("Project Beta"));
            Assert.That(description, Is.EqualTo("This is another sample project."));
            Assert.That(selectedGroupIds.Count, Is.EqualTo(2));
            Assert.That(allGroups.Count, Is.EqualTo(1));
        }

    }
}
