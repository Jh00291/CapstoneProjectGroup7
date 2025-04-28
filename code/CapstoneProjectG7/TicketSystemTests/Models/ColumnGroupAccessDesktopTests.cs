using NUnit.Framework;
using TicketSystemDesktop.Models;

namespace TicketSystemTests.Models
{
    [TestFixture]
    public class ColumnGroupAccessDesktopTests
    {
        [Test]
        public void ColumnGroupAccess_DefaultValues_AreCorrect()
        {
            var access = new ColumnGroupAccess
            {
                Id = 1,
                KanbanColumnId = 10,
                GroupId = 20
            };

            Assert.That(access.Id, Is.EqualTo(1));
            Assert.That(access.KanbanColumnId, Is.EqualTo(10));
            Assert.That(access.GroupId, Is.EqualTo(20));
            Assert.That(access.KanbanColumn, Is.Null);
            Assert.That(access.Group, Is.Null);
        }

        [Test]
        public void ColumnGroupAccess_CanSetKanbanColumnAndGroup()
        {
            var column = new KanbanColumn { Id = 99, Name = "Test Column" };
            var group = new Group { Id = 50, Name = "Test Group" };

            var access = new ColumnGroupAccess
            {
                KanbanColumn = column,
                Group = group
            };

            Assert.That(access.KanbanColumn, Is.Not.Null);
            Assert.That(access.KanbanColumn.Id, Is.EqualTo(99));
            Assert.That(access.Group, Is.Not.Null);
            Assert.That(access.Group.Id, Is.EqualTo(50));
        }
    }
}
