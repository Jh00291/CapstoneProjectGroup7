using TicketSystemWeb.Models.ProjectManagement.Project;

namespace TicketSystemWeb.Models.ProjectManagement.Group
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public List<Employee.EmployeeGroup> EmployeeGroups { get; set; } = new();

        public string? ManagerId { get; set; } = string.Empty;
        public Employee.Employee? Manager { get; set; }

        public List<ProjectGroup> ProjectGroups { get; set; } = new();
    }

}
