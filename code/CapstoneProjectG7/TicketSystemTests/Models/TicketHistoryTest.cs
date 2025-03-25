using NUnit.Framework;
using System;
using TicketSystemWeb.Models.KanbanBoard;

namespace TicketSystemWeb.Tests
{
    public class TicketHistoryTests
    {
        [Test]
        public void CanCreateTicketHistory()
        {
            var history = new TicketHistory
            {
                TicketId = 1,
                Action = "Ticket created",
                PerformedBy = "admin"
            };

            Assert.That(1, Is.EqualTo(history.TicketId));
            Assert.That("Ticket created", Is.EqualTo(history.Action));
            Assert.That("admin", Is.EqualTo(history.PerformedBy));
        }
    }
}
