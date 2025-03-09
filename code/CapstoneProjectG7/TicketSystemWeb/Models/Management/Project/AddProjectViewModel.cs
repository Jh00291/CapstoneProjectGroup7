using System.ComponentModel.DataAnnotations;
using TicketSystemWeb.Models.Employee;
using TicketSystemWeb.Models.ProjectManagement.Group;

namespace TicketSystemWeb.ViewModels
{
    /// <summary>
    /// addproject viewmodel
    /// </summary>
    public class AddProjectViewModel
    {
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>

        [Required]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 150 characters.")]
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>

        [Required]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the project manager identifier.
        /// </summary>
        /// <value>
        /// The project manager identifier.
        /// </value>

        [Required(ErrorMessage = "Please select a Project Manager.")]
        public string ProjectManagerId { get; set; }
        /// <summary>
        /// Gets or sets the group ids.
        /// </summary>
        /// <value>
        /// The group ids.
        /// </value>
        public List<int> GroupIds { get; set; } = new();
        /// <summary>
        /// Gets or sets all employees.
        /// </summary>
        /// <value>
        /// All employees.
        /// </value>
        public List<Employee>? AllEmployees { get; set; }
        /// <summary>
        /// Gets or sets all groups.
        /// </summary>
        /// <value>
        /// All groups.
        /// </value>
        public List<Group> AllGroups { get; set; } = new();
    }
}
