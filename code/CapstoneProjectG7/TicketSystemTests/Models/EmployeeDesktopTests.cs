using NUnit.Framework;
using TicketSystemDesktop.Models;

namespace TicketSystemTests.Models
{
    [TestFixture]
    public class EmployeeDesktopTests
    {
        [Test]
        public void Employee_DefaultProperties_AreAccessible()
        {
            var employee = new Employee
            {
                Id = "e123",
                UserName = "jane.doe",
                Email = "jane@example.com"
            };

            Assert.That(employee.Id, Is.EqualTo("e123"));
            Assert.That(employee.UserName, Is.EqualTo("jane.doe"));
            Assert.That(employee.Email, Is.EqualTo("jane@example.com"));
        }
    }
}
