using NUnit.Framework;
using System.Collections.Generic;
using TicketSystemWeb.Models.ProjectManagement.Group;
using TicketSystemWeb.Models.Employee;
using TicketSystemWeb.Models.ProjectManagement.Project;

namespace TicketSystemWeb.Tests.Models
{
    [TestFixture]
    public class GroupTests
    {
        [Test]
        public void Group_DefaultValuesAreCorrect()
        {
            // Arrange
            var group = new Group();

            // Assert
            Assert.That(group.Id, Is.EqualTo(0));
            Assert.That(group.Name, Is.EqualTo(string.Empty));
            Assert.That(group.ManagerId, Is.EqualTo(string.Empty));
            Assert.That(group.EmployeeGroups, Is.Empty);
            Assert.That(group.ProjectGroups, Is.Empty);
            Assert.That(group.Manager, Is.Null);
        }

        [Test]
        public void Group_CanAssignManager()
        {
            // Arrange
            var manager = new Employee { Id = "manager123", UserName = "ManagerUser" };
            var group = new Group { Manager = manager };

            // Assert
            Assert.That(group.Manager, Is.Not.Null);
            Assert.That(group.Manager.Id, Is.EqualTo("manager123"));
            Assert.That(group.Manager.UserName, Is.EqualTo("ManagerUser"));
        }


        [Test]
        public void Group_CanAssignValues()
        {
            // Arrange
            var manager = new Employee { Id = "manager123", UserName = "ManagerUser" };
            var group = new Group
            {
                Id = 5,
                Name = "Software Engineers",
                ManagerId = "manager123",
                Manager = manager,
                EmployeeGroups = new List<EmployeeGroup>
                {
                    new EmployeeGroup { EmployeeId = "emp1", GroupId = 5 },
                    new EmployeeGroup { EmployeeId = "emp2", GroupId = 5 }
                },
                ProjectGroups = new List<ProjectGroup>
                {
                    new ProjectGroup { ProjectId = 101, GroupId = 5 },
                    new ProjectGroup { ProjectId = 102, GroupId = 5 }
                }
            };

            // Assert
            Assert.That(group.Id, Is.EqualTo(5));
            Assert.That(group.Name, Is.EqualTo("Software Engineers"));
            Assert.That(group.ManagerId, Is.EqualTo("manager123"));
            Assert.That(group.Manager, Is.SameAs(manager));
            Assert.That(group.EmployeeGroups.Count, Is.EqualTo(2));
            Assert.That(group.ProjectGroups.Count, Is.EqualTo(2));
        }
    }
}
