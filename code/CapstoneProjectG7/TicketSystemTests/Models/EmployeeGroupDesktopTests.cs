using NUnit.Framework;
using TicketSystemDesktop.Models;

namespace TicketSystemTests.Models
{
    [TestFixture]
    public class EmployeeGroupDesktopTests
    {
        [Test]
        public void EmployeeGroup_DefaultValues_AreCorrect()
        {
            var employeeGroup = new EmployeeGroup
            {
                EmployeeId = "emp001",
                GroupId = 123
            };

            Assert.That(employeeGroup.EmployeeId, Is.EqualTo("emp001"));
            Assert.That(employeeGroup.GroupId, Is.EqualTo(123));
            Assert.That(employeeGroup.Employee, Is.Null);
            Assert.That(employeeGroup.Group, Is.Null);
        }

        [Test]
        public void EmployeeGroup_CanSetEmployeeAndGroup()
        {
            var employee = new Employee { Id = "emp002", UserName = "jane.doe" };
            var group = new Group { Id = 456, Name = "Group B" };

            var employeeGroup = new EmployeeGroup
            {
                Employee = employee,
                Group = group
            };

            Assert.That(employeeGroup.Employee, Is.Not.Null);
            Assert.That(employeeGroup.Employee.Id, Is.EqualTo("emp002"));
            Assert.That(employeeGroup.Group, Is.Not.Null);
            Assert.That(employeeGroup.Group.Id, Is.EqualTo(456));
        }
    }
}
