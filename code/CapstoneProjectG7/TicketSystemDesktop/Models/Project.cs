using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketSystemDesktop.Models
{
    /// <summary>
    /// the project class
    /// </summary>
    public class Project
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the kanban board.
        /// </summary>
        /// <value>
        /// The kanban board.
        /// </value>
        public KanbanBoard? KanbanBoard { get; set; }

        /// <summary>
        /// Gets or sets the tickets.
        /// </summary>
        /// <value>
        /// The tickets.
        /// </value>
        public ICollection<Ticket> Tickets { get; set; } = new List<Ticket>();
    }


}
