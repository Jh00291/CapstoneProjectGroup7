using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TicketSystemWeb.Models.ProjectManagement.Group;

namespace TicketSystemWeb.Models.Employee
{
    /// <summary>
    /// employee group
    /// </summary>
    public class EmployeeGroup
    {
        /// <summary>
        /// Gets or sets the employee identifier.
        /// </summary>
        /// <value>
        /// The employee identifier.
        /// </value>
        [Key, Column(Order = 0)]
        public string EmployeeId { get; set; }
        /// <summary>
        /// Gets or sets the employee.
        /// </summary>
        /// <value>
        /// The employee.
        /// </value>
        public Employee Employee { get; set; } = null!;
        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        [Key, Column(Order = 1)]
        public int GroupId { get; set; }
        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        public Group Group { get; set; } = null!;
    }

}
