using TicketSystemWeb.Models.ProjectManagement.Group;

namespace TicketSystemWeb.ViewModels
{
    public class EditProjectViewModel
    {
        public int ProjectId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<int> SelectedGroupIds { get; set; } = new();
        public List<Group> AllGroups { get; set; } = new();
    }
}
