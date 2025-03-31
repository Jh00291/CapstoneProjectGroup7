using TicketSystemWeb.Models.ProjectManagement.Group;

namespace TicketSystemWeb.Models.KanbanBoard
{
    /// <summary>
    /// kanban column
    /// </summary>
    public class KanbanColumn
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the order.
        /// </summary>
        /// <value>
        /// The order.
        /// </value>
        public int Order { get; set; }

        /// <summary>
        /// Gets or sets the kanban board identifier.
        /// </summary>
        /// <value>
        /// The kanban board identifier.
        /// </value>
        public int KanbanBoardId { get; set; }

        /// <summary>
        /// Gets or sets the kanban board.
        /// </summary>
        /// <value>
        /// The kanban board.
        /// </value>
        public KanbanBoard KanbanBoard { get; set; }

        /// <summary>
        /// Gets or sets the group access.
        /// </summary>
        /// <value>
        /// The group access.
        /// </value>
        public ICollection<ColumnGroupAccess> GroupAccess { get; set; }
    }

    public class ColumnGroupAccess
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the kanban column identifier.
        /// </summary>
        /// <value>
        /// The kanban column identifier.
        /// </value>
        public int KanbanColumnId { get; set; }

        /// <summary>
        /// Gets or sets the kanban column.
        /// </summary>
        /// <value>
        /// The kanban column.
        /// </value>
        public KanbanColumn KanbanColumn { get; set; }

        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        public Group Group { get; set; }
    }

}
