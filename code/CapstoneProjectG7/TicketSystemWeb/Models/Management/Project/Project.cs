namespace TicketSystemWeb.Models.ProjectManagement.Project
{
    public class Project
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        public List<ProjectGroup> ProjectGroups { get; set; } = new();

        public string? ProjectManagerId { get; set; }
        public Employee.Employee? ProjectManager { get; set; }
    }

}
