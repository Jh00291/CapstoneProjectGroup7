using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketSystemDesktop.Models
{
    /// <summary>
    /// Represents the association between an employee and a group.
    /// </summary>
    public class EmployeeGroup
    {
        /// <summary>
        /// Gets or sets the employee id.
        /// </summary>
        public string EmployeeId { get; set; }

        /// <summary>
        /// Gets or sets the employee.
        /// </summary>
        public Employee Employee { get; set; }

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
