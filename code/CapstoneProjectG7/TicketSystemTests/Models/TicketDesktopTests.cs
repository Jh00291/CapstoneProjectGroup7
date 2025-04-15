using NUnit.Framework;
using System;
using TicketSystemDesktop.Models;

namespace TicketSystemTests.Models
{
    [TestFixture]
    public class TicketDesktopTests
    {
        [Test]
        public void Ticket_DefaultValues_AreCorrect()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var ticket = new Ticket
            {
                TicketId = 1,
                Title = "Test Title",
                Description = "This is a sample description.",
                Status = "Open",
                CreatedBy = "tester",
                ProjectId = 42
            };

            // Assert
            Assert.That(ticket.TicketId, Is.EqualTo(1));
            Assert.That(ticket.Title, Is.EqualTo("Test Title"));
            Assert.That(ticket.Description, Is.EqualTo("This is a sample description."));
            Assert.That(ticket.Status, Is.EqualTo("Open"));
            Assert.That(ticket.CreatedBy, Is.EqualTo("tester"));
            Assert.That(ticket.ProjectId, Is.EqualTo(42));
            Assert.That(ticket.CreatedAt, Is.LessThanOrEqualTo(DateTime.UtcNow));
            Assert.That(ticket.ClosedAt, Is.Null);
            Assert.That(ticket.Project, Is.Null);
            Assert.That(ticket.AssignedToId, Is.Null);
            Assert.That(ticket.AssignedTo, Is.Null);
        }

        [Test]
        public void Ticket_CanSetClosedAt()
        {
            // Arrange
            var closedDate = DateTime.UtcNow;
            var ticket = new Ticket
            {
                ClosedAt = closedDate
            };

            // Assert
            Assert.That(ticket.ClosedAt, Is.EqualTo(closedDate));
        }

        [Test]
        public void Ticket_CanSetProject()
        {
            // Arrange
            var project = new Project
            {
                Id = 101,
                Title = "Sample Project"
            };

            var ticket = new Ticket
            {
                Project = project,
                ProjectId = project.Id
            };

            // Assert
            Assert.That(ticket.Project, Is.Not.Null);
            Assert.That(ticket.Project.Id, Is.EqualTo(101));
            Assert.That(ticket.Project.Title, Is.EqualTo("Sample Project"));
        }

        [Test]
        public void Ticket_CanSetAssignedTo()
        {
            // Arrange
            var employee = new Employee
            {
                Id = "emp123",
                UserName = "john.doe"
            };

            var ticket = new Ticket
            {
                AssignedTo = employee,
                AssignedToId = employee.Id
            };

            // Assert
            Assert.That(ticket.AssignedTo, Is.Not.Null);
            Assert.That(ticket.AssignedTo.UserName, Is.EqualTo("john.doe"));
            Assert.That(ticket.AssignedToId, Is.EqualTo("emp123"));
        }
    }
}
