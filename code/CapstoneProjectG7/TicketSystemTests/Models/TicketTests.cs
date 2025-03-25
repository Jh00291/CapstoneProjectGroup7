using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
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
            Assert.That(ticket.Comments, Is.Empty);
            Assert.That(ticket.History, Is.Empty);
        }

        [Test]
        public void Ticket_CanSetClosedAtDate()
        {
            // Arrange
            var now = DateTime.UtcNow;
            var ticket = new Ticket { ClosedAt = now };

            // Assert
            Assert.That(ticket.ClosedAt, Is.EqualTo(now));
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
                ProjectGroups = new List<ProjectGroup>(),
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

        [Test]
        public void Ticket_CanAddComments()
        {
            // Arrange
            var ticket = new Ticket
            {
                TicketId = 2,
                Title = "With Comment"
            };

            ticket.Comments.Add(new TicketComment
            {
                TicketId = ticket.TicketId,
                CommentText = "This is a comment.",
                AuthorName = "admin"
            });

            // Assert
            Assert.That(ticket.Comments.Count, Is.EqualTo(1));
            var comment = ticket.Comments.First();
            Assert.That(comment.CommentText, Is.EqualTo("This is a comment."));
            Assert.That(comment.AuthorName, Is.EqualTo("admin"));
            Assert.That(comment.TicketId, Is.EqualTo(2));
        }

        [Test]
        public void Ticket_CanAddHistoryEntries()
        {
            // Arrange
            var ticket = new Ticket
            {
                TicketId = 3,
                Title = "With History"
            };

            ticket.History.Add(new TicketHistory
            {
                TicketId = ticket.TicketId,
                Action = "Moved to 'In Progress'",
                PerformedBy = "admin"
            });

            // Assert
            Assert.That(ticket.History.Count, Is.EqualTo(1));
            var history = ticket.History.First();
            Assert.That(history.Action, Is.EqualTo("Moved to 'In Progress'"));
            Assert.That(history.PerformedBy, Is.EqualTo("admin"));
            Assert.That(history.TicketId, Is.EqualTo(3));
        }
    }
}
