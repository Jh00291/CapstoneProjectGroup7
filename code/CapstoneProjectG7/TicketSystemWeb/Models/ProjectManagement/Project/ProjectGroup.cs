namespace TicketSystemWeb.Models.ProjectManagement.Project
{
    public class ProjectGroup
    {
        public int ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public int GroupId { get; set; }
        public Group.Group Group { get; set; } = null!;
    }

}
