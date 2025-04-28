using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketSystemDesktop.Models
{
    /// <summary>
    /// Represents group access permissions for a kanban column.
    /// </summary>
    public class ColumnGroupAccess
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the kanban column id.
        /// </summary>
        public int KanbanColumnId { get; set; }

        /// <summary>
        /// Gets or sets the kanban column.
        /// </summary>
        public KanbanColumn KanbanColumn { get; set; }

        /// <summary>
        /// Gets or sets the group id.
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        public Group Group { get; set; }
    }
}
