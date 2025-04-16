using NUnit.Framework;
using TicketSystemDesktop.Models;
using System.Collections.Generic;

namespace TicketSystemTests.Models
{
    [TestFixture]
    public class ProjectDesktopTests
    {
        [Test]
        public void Project_DefaultValues_AreCorrect()
        {
            var project = new Project();

            Assert.That(project.Id, Is.EqualTo(0));
            Assert.That(project.Title, Is.EqualTo(string.Empty));
            Assert.That(project.KanbanBoard, Is.Null);
            Assert.That(project.Tickets, Is.Not.Null);
            Assert.That(project.Tickets.Count, Is.EqualTo(0));
        }

        [Test]
        public void Project_CanSetValues()
        {
            var board = new KanbanBoard();
            var ticketList = new List<Ticket> { new Ticket { Title = "Bug Fix" } };

            var project = new Project
            {
                Id = 10,
                Title = "Desktop App",
                KanbanBoard = board,
                Tickets = ticketList
            };

            Assert.That(project.Id, Is.EqualTo(10));
            Assert.That(project.Title, Is.EqualTo("Desktop App"));
            Assert.That(project.KanbanBoard, Is.EqualTo(board));
            Assert.That(project.Tickets, Is.EqualTo(ticketList));
        }
    }
}
