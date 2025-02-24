using TicketSystemWeb.Models.ProjectManagement.Group;

namespace TicketSystemWeb.ViewModels
{
    public class AddProjectViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<int> GroupIds { get; set; } = new();
        public List<Group> AllGroups { get; set; } = new();
    }
}
