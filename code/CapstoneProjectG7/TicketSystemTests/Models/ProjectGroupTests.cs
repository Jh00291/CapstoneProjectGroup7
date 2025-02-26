using NUnit.Framework;
using TicketSystemWeb.Models.ProjectManagement.Project;
using TicketSystemWeb.Models.ProjectManagement.Group;

namespace TicketSystemWeb.Tests.Models
{
    [TestFixture]
    public class ProjectGroupTests
    {
        [Test]
        public void ProjectGroup_DefaultValues_AreCorrect()
        {
            // Arrange
            var projectGroup = new ProjectGroup();

            // Assert
            Assert.That(projectGroup.Project, Is.Null);
            Assert.That(projectGroup.Group, Is.Null);
        }

        [Test]
        public void ProjectGroup_CanAssignValues()
        {
            // Arrange
            var project = new Project { Id = 200, Title = "Test Project" };
            var group = new Group { Id = 300, Name = "Engineering Team" };

            var projectGroup = new ProjectGroup
            {
                ProjectId = 200,
                GroupId = 300,
                Project = project,
                Group = group
            };

            // Assert
            Assert.That(projectGroup.ProjectId, Is.EqualTo(200));
            Assert.That(projectGroup.GroupId, Is.EqualTo(300));
            Assert.That(projectGroup.Project, Is.SameAs(project));
            Assert.That(projectGroup.Group, Is.SameAs(group));
        }
    }
}
