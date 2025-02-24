using NUnit.Framework;
using TicketSystemWeb.Models.Employee;
using TicketSystemWeb.Models.ProjectManagement.Group;

namespace TicketSystemWeb.Tests.Models
{
    [TestFixture]
    public class EmployeeGroupTests
    {
        [Test]
        public void EmployeeGroup_DefaultValuesAreCorrect()
        {
            // Arrange & Act
            var employeeGroup = new EmployeeGroup();

            // Assert
            Assert.That(employeeGroup.Employee, Is.Null);
            Assert.That(employeeGroup.Group, Is.Null);
        }

        [Test]
        public void EmployeeGroup_CanAssignValues()
        {
            // Arrange
            var employee = new Employee { Id = "emp123", UserName = "TestUser" };
            var group = new Group { Id = 1, Name = "Development" }; // Assuming Group has Id & Name
            var employeeGroup = new EmployeeGroup
            {
                EmployeeId = "emp123",
                GroupId = 1,
                Employee = employee,
                Group = group
            };

            // Assert
            Assert.That(employeeGroup.EmployeeId, Is.EqualTo("emp123"));
            Assert.That(employeeGroup.GroupId, Is.EqualTo(1));
            Assert.That(employeeGroup.Employee, Is.SameAs(employee));
            Assert.That(employeeGroup.Group, Is.SameAs(group));
        }
    }
}
