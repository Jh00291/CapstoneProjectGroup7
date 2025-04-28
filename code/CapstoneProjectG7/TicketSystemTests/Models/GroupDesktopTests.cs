using NUnit.Framework;
using System.Collections.Generic;
using TicketSystemDesktop.Models;

namespace TicketSystemTests.Models
{
    [TestFixture]
    public class GroupDesktopTests
    {
        [Test]
        public void Group_DefaultValues_AreCorrect()
        {
            var group = new Group
            {
                Id = 1,
                Name = "Test Group",
                ManagerId = "manager001"
            };

            Assert.That(group.Id, Is.EqualTo(1));
            Assert.That(group.Name, Is.EqualTo("Test Group"));
            Assert.That(group.ManagerId, Is.EqualTo("manager001"));
            Assert.That(group.EmployeeGroups, Is.Empty);
            Assert.That(group.ProjectGroups, Is.Empty);
            Assert.That(group.ColumnGroupAccesses, Is.Empty);
        }

        [Test]
        public void Group_CanSetCollections()
        {
            var employeeGroups = new List<EmployeeGroup> { new EmployeeGroup { EmployeeId = "emp001" } };
            var projectGroups = new List<ProjectGroup> { new ProjectGroup { ProjectId = 100 } };
            var columnAccesses = new List<ColumnGroupAccess> { new ColumnGroupAccess { Id = 200 } };

            var group = new Group
            {
                EmployeeGroups = employeeGroups,
                ProjectGroups = projectGroups,
                ColumnGroupAccesses = columnAccesses
            };

            Assert.That(group.EmployeeGroups, Is.Not.Empty);
            Assert.That(group.ProjectGroups, Is.Not.Empty);
            Assert.That(group.ColumnGroupAccesses, Is.Not.Empty);
        }
    }
}
