using NUnit.Framework;
using System;
using System.Collections.Generic;
using TicketSystemWeb.Models.KanbanBoard;
using TicketSystemWeb.Models.ProjectManagement.Project;

namespace TicketSystemWeb.Tests.Models
{
    [TestFixture]
    public class TicketTests
    {
        [Test]
        public void Ticket_DefaultValuesAreCorrect()
        {
            // Arrange
            var ticket = new Ticket
            {
                TicketId = 1,
                Title = "Sample Ticket",
                Description = "This is a test ticket",
                Status = "Open",
                CreatedBy = "JohnDoe"
            };

            // Assert
            Assert.That(ticket.TicketId, Is.EqualTo(1));
            Assert.That(ticket.Title, Is.EqualTo("Sample Ticket"));
            Assert.That(ticket.Description, Is.EqualTo("This is a test ticket"));
            Assert.That(ticket.Status, Is.EqualTo("Open"));
            Assert.That(ticket.CreatedBy, Is.EqualTo("JohnDoe"));
            Assert.That(ticket.CreatedAt, Is.LessThanOrEqualTo(DateTime.UtcNow));
            Assert.That(ticket.ClosedAt, Is.Null);
        }

        [Test]
        public void Ticket_CanSetClosedAtDate()
        {
            // Arrange
            var ticket = new Ticket { ClosedAt = DateTime.UtcNow };

            // Assert
            Assert.That(ticket.ClosedAt, Is.Not.Null);
        }

        [Test]
        public void Ticket_CanAssignAndRetrieveProject()
        {
            // Arrange
            var project = new Project
            {
                Id = 1,
                Title = "Test Project",
                Description = "This is a test project",
                ProjectManagerId = "PM001",
                ProjectGroups = new List<ProjectGroup>(), // Assuming ProjectGroup is properly defined
                Tickets = new List<Ticket>()
            };

            var ticket = new Ticket
            {
                Project = project,
                ProjectId = project.Id
            };

            // Assert
            Assert.That(ticket.Project, Is.Not.Null);
            Assert.That(ticket.Project.Id, Is.EqualTo(1));
            Assert.That(ticket.Project.Title, Is.EqualTo("Test Project"));
            Assert.That(ticket.Project.Description, Is.EqualTo("This is a test project"));
            Assert.That(ticket.Project.ProjectManagerId, Is.EqualTo("PM001"));
        }
    }
}
