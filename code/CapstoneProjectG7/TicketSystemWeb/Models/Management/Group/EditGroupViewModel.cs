using System.ComponentModel.DataAnnotations;
using TicketSystemWeb.Models.Employee;

namespace TicketSystemWeb.ViewModels
{
    /// <summary>
    /// editgroup viewmodel
    /// </summary>
    public class EditGroupViewModel
    {
        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        public int GroupId { get; set; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>

        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Group name must be between 3 and 100 characters.")]
        public string Name { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the selected manager identifier.
        /// </summary>
        /// <value>
        /// The selected manager identifier.
        /// </value>

        [Required(ErrorMessage = "Please select a manager.")]
        public string SelectedManagerId { get; set; }
        /// <summary>
        /// Gets or sets all employees.
        /// </summary>
        /// <value>
        /// All employees.
        /// </value>
        public List<Employee> AllEmployees { get; set; } = new();
        /// <summary>
        /// Gets or sets the selected employee ids.
        /// </summary>
        /// <value>
        /// The selected employee ids.
        /// </value>
        public List<string> SelectedEmployeeIds { get; set; } = new();
    }
}
