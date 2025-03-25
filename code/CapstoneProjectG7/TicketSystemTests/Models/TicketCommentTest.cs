using NUnit.Framework;
using System;
using TicketSystemWeb.Models.KanbanBoard;

namespace TicketSystemWeb.Tests
{
    public class TicketCommentTests
    {
        [Test]
        public void CanCreateTicketComment()
        {
            var comment = new TicketComment
            {
                TicketId = 2,
                CommentText = "This is a comment.",
                AuthorName = "johndoe"
            };

            Assert.That(2, Is.EqualTo(comment.TicketId));
            Assert.That("This is a comment.", Is.EqualTo(comment.CommentText));
            Assert.That("johndoe", Is.EqualTo(comment.AuthorName));
        }
    }
}
