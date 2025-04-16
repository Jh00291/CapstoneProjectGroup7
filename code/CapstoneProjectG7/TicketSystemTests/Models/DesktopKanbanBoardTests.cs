using NUnit.Framework;
using TicketSystemDesktop.Models;
using System.Collections.Generic;

namespace TicketSystemTests.Models
{
    [TestFixture]
    public class DesktopKanbanBoardTests
    {
        [Test]
        public void KanbanBoard_DefaultValues_AreCorrect()
        {
            var board = new KanbanBoard();

            Assert.That(board.Id, Is.EqualTo(0));
            Assert.That(board.ProjectName, Is.EqualTo(string.Empty));
            Assert.That(board.ProjectId, Is.EqualTo(0));
            Assert.That(board.Project, Is.Null);
            Assert.That(board.Columns, Is.Null); // Default is not initialized
        }

        [Test]
        public void KanbanBoard_CanSetValues()
        {
            var project = new Project { Title = "Kanban Project" };
            var columns = new List<KanbanColumn>
            {
                new KanbanColumn { Name = "To Do" },
                new KanbanColumn { Name = "Done" }
            };

            var board = new KanbanBoard
            {
                Id = 1,
                ProjectName = "Kanban Demo",
                ProjectId = 123,
                Project = project,
                Columns = columns
            };

            Assert.That(board.Id, Is.EqualTo(1));
            Assert.That(board.ProjectName, Is.EqualTo("Kanban Demo"));
            Assert.That(board.ProjectId, Is.EqualTo(123));
            Assert.That(board.Project.Title, Is.EqualTo("Kanban Project"));
            Assert.That(board.Columns.Count, Is.EqualTo(2));
        }
    }
}
