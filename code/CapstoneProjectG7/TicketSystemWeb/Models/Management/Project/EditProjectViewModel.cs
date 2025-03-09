using TicketSystemWeb.Models.ProjectManagement.Group;
using TicketSystemWeb.Models.Employee;
using System.ComponentModel.DataAnnotations;

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
        [Required]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 150 characters.")]
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the description.
        /// </summary>

        [Required]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the project manager ID.
        /// </summary>
        [Required(ErrorMessage = "Please select a Project Manager.")]
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
