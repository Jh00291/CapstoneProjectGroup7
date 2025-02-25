using TicketSystemWeb.Models.ProjectManagement.Group;
using TicketSystemWeb.Models.Employee;

namespace TicketSystemWeb.ViewModels
{
    /// <summary>
    /// Edit Project ViewModel
    /// </summary>
    public class EditProjectViewModel
    {
        /// <summary>
        /// Gets or sets the project identifier.
        /// </summary>
        public int ProjectId { get; set; }
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the project manager ID.
        /// </summary>
        public string ProjectManagerId { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the list of selected group IDs.
        /// </summary>
        public List<int> SelectedGroupIds { get; set; } = new();
        /// <summary>
        /// Gets or sets all groups available for selection.
        /// </summary>
        public List<Group> AllGroups { get; set; } = new();
        /// <summary>
        /// Gets or sets all employees available for manager selection.
        /// </summary>
        public List<Employee> AllEmployees { get; set; } = new();
    }
}
