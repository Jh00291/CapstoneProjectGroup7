using System.ComponentModel.DataAnnotations;
using TicketSystemWeb.Models.Employee;

namespace TicketSystemWeb.ViewModels
{
    public class AddGroupViewModel
    {
        [Required]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Group name must be between 3 and 100 characters.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Please select a manager.")]
        public string SelectedManagerId { get; set; }

        public List<string> SelectedEmployeeIds { get; set; } = new List<string>();

        public List<Employee> AllEmployees { get; set; } = new List<Employee>();
    }
}
