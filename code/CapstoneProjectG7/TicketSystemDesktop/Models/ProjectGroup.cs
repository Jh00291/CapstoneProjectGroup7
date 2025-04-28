using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketSystemDesktop.Models
{
    /// <summary>
    /// Represents the association between a project and a group.
    /// </summary>
    public class ProjectGroup
    {
        /// <summary>
        /// Gets or sets the project id.
        /// </summary>
        public int ProjectId { get; set; }

        /// <summary>
        /// Gets or sets the project.
        /// </summary>
        public Project Project { get; set; }

        /// <summary>
        /// Gets or sets the group id.
        /// </summary>
        public int GroupId { get; set; }

        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        public Group Group { get; set; }

        /// <summary>
        /// Gets or sets whether the group is approved for the project.
        /// </summary>
        public bool IsApproved { get; set; }
    }
}

