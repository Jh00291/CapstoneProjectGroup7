namespace TicketSystemWeb.ViewModels
{
    public class EditEmployeeViewModel
    {
        public string Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "user";
    }
}
