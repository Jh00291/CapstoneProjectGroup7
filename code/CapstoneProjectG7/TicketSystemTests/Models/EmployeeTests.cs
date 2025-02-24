using NUnit.Framework;
using System.Collections.Generic;
using TicketSystemWeb.Models.Employee;

namespace TicketSystemWeb.Tests.Models
{
    [TestFixture]
    public class EmployeeTests
    {
        [Test]
        public void Employee_DefaultValuesAreCorrect()
        {
            // Arrange & Act
            var employee = new Employee();

            // Assert
            Assert.That(employee.EmployeeGroups, Is.Not.Null);
            Assert.That(employee.EmployeeGroups.Count, Is.EqualTo(0));
        }

        [Test]
        public void Employee_CanAssignValues()
        {
            // Arrange
            var employee = new Employee
            {
                Id = "emp123",
                UserName = "employee1",
                Email = "employee@example.com",
                EmployeeGroups = new List<EmployeeGroup>
                {
                    new EmployeeGroup { EmployeeId = "emp123", GroupId = 1 },
                    new EmployeeGroup { EmployeeId = "emp123", GroupId = 2 }
                }
            };

            // Assert
            Assert.That(employee.Id, Is.EqualTo("emp123"));
            Assert.That(employee.UserName, Is.EqualTo("employee1"));
            Assert.That(employee.Email, Is.EqualTo("employee@example.com"));
            Assert.That(employee.EmployeeGroups.Count, Is.EqualTo(2));
        }
    }
}
