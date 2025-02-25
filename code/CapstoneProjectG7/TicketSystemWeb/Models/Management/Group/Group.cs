using TicketSystemWeb.Models.ProjectManagement.Project;

namespace TicketSystemWeb.Models.ProjectManagement.Group
{
    /// <summary>
    /// the group
    /// </summary>
    public class Group
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
        /// Gets or sets the employee groups.
        /// </summary>
        /// <value>
        /// The employee groups.
        /// </value>
        public List<Employee.EmployeeGroup> EmployeeGroups { get; set; } = new();
        /// <summary>
        /// Gets or sets the manager identifier.
        /// </summary>
        /// <value>
        /// The manager identifier.
        /// </value>
        public string? ManagerId { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the manager.
        /// </summary>
        /// <value>
        /// The manager.
        /// </value>
        public Employee.Employee? Manager { get; set; }
        /// <summary>
        /// Gets or sets the project groups.
        /// </summary>
        /// <value>
        /// The project groups.
        /// </value>
        public List<ProjectGroup> ProjectGroups { get; set; } = new();
    }

}
