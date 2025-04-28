using NUnit.Framework;
using TicketSystemDesktop.Models;

namespace TicketSystemTests.Models
{
    [TestFixture]
    public class ProjectGroupDesktopTests
    {
        [Test]
        public void ProjectGroup_DefaultValues_AreCorrect()
        {
            var projectGroup = new ProjectGroup
            {
                ProjectId = 5,
                GroupId = 10,
                IsApproved = true
            };

            Assert.That(projectGroup.ProjectId, Is.EqualTo(5));
            Assert.That(projectGroup.GroupId, Is.EqualTo(10));
            Assert.That(projectGroup.IsApproved, Is.True);
            Assert.That(projectGroup.Project, Is.Null);
            Assert.That(projectGroup.Group, Is.Null);
        }

        [Test]
        public void ProjectGroup_CanSetProjectAndGroup()
        {
            var project = new Project { Id = 88, Title = "Test Project" };
            var group = new Group { Id = 33, Name = "Test Group" };

            var projectGroup = new ProjectGroup
            {
                Project = project,
                Group = group
            };

            Assert.That(projectGroup.Project, Is.Not.Null);
            Assert.That(projectGroup.Project.Id, Is.EqualTo(88));
            Assert.That(projectGroup.Group, Is.Not.Null);
            Assert.That(projectGroup.Group.Id, Is.EqualTo(33));
        }
    }
}
