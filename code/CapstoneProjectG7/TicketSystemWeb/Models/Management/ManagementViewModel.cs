using TicketSystemWeb.Models.Employee;
using TicketSystemWeb.Models.ProjectManagement.Group;
using TicketSystemWeb.Models.ProjectManagement.Project;

namespace TicketSystemWeb.ViewModels
{
    /// <summary>
    /// management viewmodel
    /// </summary>
    public class ManagementViewModel
    {
        /// <summary>
        /// Gets or sets the projects.
        /// </summary>
        /// <value>
        /// The projects.
        /// </value>
        public List<Project> Projects { get; set; } = new();
        /// <summary>
        /// Gets or sets the groups.
        /// </summary>
        /// <value>
        /// The groups.
        /// </value>
        public List<Group> Groups { get; set; } = new();
        /// <summary>
        /// Gets or sets all employees.
        /// </summary>
        /// <value>
        /// All employees.
        /// </value>
        public List<Employee> AllEmployees { get; set; } = new();
        /// <summary>
        /// Gets or sets a value indicating whether this instance can add project.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can add project; otherwise, <c>false</c>.
        /// </value>
        public bool CanAddProject { get; set; }
        /// <summary>
        /// Gets or sets a value indicating whether this instance can manage groups.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this instance can manage groups; otherwise, <c>false</c>.
        /// </value>
        public bool CanManageGroups { get; set; }

        public List<int> SelectedGroupIds { get; set; } = new();

    }
}
