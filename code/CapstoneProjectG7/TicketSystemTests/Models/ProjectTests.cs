using NUnit.Framework;
using System.Collections.Generic;
using TicketSystemWeb.Models.ProjectManagement.Project;
using TicketSystemWeb.Models.Employee;

namespace TicketSystemWeb.Tests.Models
{
    [TestFixture]
    public class ProjectTests
    {
        [Test]
        public void Project_DefaultValues_AreCorrect()
        {
            // Arrange
            var project = new Project();

            // Assert
            Assert.That(project.Id, Is.EqualTo(0));
            Assert.That(project.Title, Is.EqualTo(string.Empty));
            Assert.That(project.Description, Is.EqualTo(string.Empty));
            Assert.That(project.ProjectGroups, Is.Empty);
            Assert.That(project.ProjectManagerId, Is.Null);
            Assert.That(project.ProjectManager, Is.Null);
        }

        [Test]
        public void Project_CanAssignValues()
        {
            // Arrange
            var manager = new Employee { Id = "manager123", UserName = "ProjectManager" };
            var project = new Project
            {
                Id = 100,
                Title = "Big Project",
                Description = "This project involves many teams.",
                ProjectManagerId = "manager123",
                ProjectManager = manager,
                ProjectGroups = new List<ProjectGroup> { new ProjectGroup { ProjectId = 100, GroupId = 1 } }
            };

            // Assert
            Assert.That(project.Id, Is.EqualTo(100));
            Assert.That(project.Title, Is.EqualTo("Big Project"));
            Assert.That(project.Description, Is.EqualTo("This project involves many teams."));
            Assert.That(project.ProjectManagerId, Is.EqualTo("manager123"));
            Assert.That(project.ProjectManager, Is.SameAs(manager));
            Assert.That(project.ProjectGroups.Count, Is.EqualTo(1));
        }
    }
}
