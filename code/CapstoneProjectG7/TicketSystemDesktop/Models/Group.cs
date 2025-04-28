using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketSystemDesktop.Models
{
    /// <summary>
    /// Represents a group of employees and project associations.
    /// </summary>
    public class Group
    {
        /// <summary>
        /// Gets or sets the id.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the group name.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the manager id.
        /// </summary>
        public string? ManagerId { get; set; }

        /// <summary>
        /// Gets or sets the employee groups.
        /// </summary>
        public ICollection<EmployeeGroup> EmployeeGroups { get; set; } = new List<EmployeeGroup>();

        /// <summary>
        /// Gets or sets the project groups.
        /// </summary>
        public ICollection<ProjectGroup> ProjectGroups { get; set; } = new List<ProjectGroup>();

        /// <summary>
        /// Gets or sets the column group accesses.
        /// </summary>
        public ICollection<ColumnGroupAccess> ColumnGroupAccesses { get; set; } = new List<ColumnGroupAccess>();
    }
}
