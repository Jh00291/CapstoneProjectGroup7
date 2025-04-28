using NUnit.Framework;
using System;
using TicketSystemDesktop.Models;

namespace TicketSystemTests.Models
{
    [TestFixture]
    public class TicketCommentDesktopTests
    {
        [Test]
        public void TicketComment_DefaultValues_AreCorrect()
        {
            var now = DateTime.UtcNow;
            var comment = new TicketComment
            {
                Id = 1,
                TicketId = 42,
                CommentText = "Sample comment",
                AuthorName = "Jane Doe"
            };

            Assert.That(comment.Id, Is.EqualTo(1));
            Assert.That(comment.TicketId, Is.EqualTo(42));
            Assert.That(comment.CommentText, Is.EqualTo("Sample comment"));
            Assert.That(comment.AuthorName, Is.EqualTo("Jane Doe"));
            Assert.That(comment.CreatedAt, Is.LessThanOrEqualTo(DateTime.UtcNow));
            Assert.That(comment.Ticket, Is.Null);
        }

        [Test]
        public void TicketComment_CanSetTicket()
        {
            var ticket = new Ticket { TicketId = 101, Title = "Bug Report" };

            var comment = new TicketComment
            {
                Ticket = ticket,
                TicketId = ticket.TicketId
            };

            Assert.That(comment.Ticket, Is.Not.Null);
            Assert.That(comment.Ticket.TicketId, Is.EqualTo(101));
        }
    }
}
