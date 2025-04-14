using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketSystemDesktop.Models
{
    /// <summary>
    /// the kanban column class
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
        public string Name { get; set; } = string.Empty;

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
    }
}

