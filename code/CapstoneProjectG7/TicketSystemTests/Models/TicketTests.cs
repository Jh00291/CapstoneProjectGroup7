using NUnit.Framework;
using System;
using TicketSystemWeb.Models;

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
            Assert.That(ticket.CreatedAt, Is.LessThanOrEqualTo(DateTime.UtcNow)); // Should be set to current time
            Assert.That(ticket.ClosedAt, Is.Null); // Default is null
        }

        [Test]
        public void Ticket_CanSetClosedAtDate()
        {
            // Arrange
            var ticket = new Ticket { ClosedAt = DateTime.UtcNow };

            // Assert
            Assert.That(ticket.ClosedAt, Is.Not.Null);
        }
    }
}
