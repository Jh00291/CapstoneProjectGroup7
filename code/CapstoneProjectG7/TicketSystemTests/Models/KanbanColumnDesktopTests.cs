using NUnit.Framework;
using TicketSystemDesktop.Models;

namespace TicketSystemTests.Models
{
    [TestFixture]
    public class KanbanColumnDesktopTests
    {
        [Test]
        public void KanbanColumn_DefaultValues_AreCorrect()
        {
            var column = new KanbanColumn();

            Assert.That(column.Id, Is.EqualTo(0));
            Assert.That(column.Name, Is.EqualTo(string.Empty));
            Assert.That(column.Order, Is.EqualTo(0));
            Assert.That(column.KanbanBoardId, Is.EqualTo(0));
            Assert.That(column.KanbanBoard, Is.Null);
        }

        [Test]
        public void KanbanColumn_CanSetValues()
        {
            var board = new KanbanBoard { ProjectName = "Demo Project" };

            var column = new KanbanColumn
            {
                Id = 5,
                Name = "In Progress",
                Order = 2,
                KanbanBoardId = 10,
                KanbanBoard = board
            };

            Assert.That(column.Id, Is.EqualTo(5));
            Assert.That(column.Name, Is.EqualTo("In Progress"));
            Assert.That(column.Order, Is.EqualTo(2));
            Assert.That(column.KanbanBoardId, Is.EqualTo(10));
            Assert.That(column.KanbanBoard.ProjectName, Is.EqualTo("Demo Project"));
        }
    }
}
