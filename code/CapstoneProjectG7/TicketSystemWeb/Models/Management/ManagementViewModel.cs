using TicketSystemWeb.Models.Employee;
using TicketSystemWeb.Models.ProjectManagement.Group;
using TicketSystemWeb.Models.ProjectManagement.Project;

namespace TicketSystemWeb.ViewModels
{
    public class ManagementViewModel
    {
        public List<Project> Projects { get; set; } = new();
        public List<Group> Groups { get; set; } = new();
        public List<Employee> AllEmployees { get; set; } = new();
        public bool CanAddProject { get; set; }
        public bool CanManageGroups { get; set; }
    }
}
