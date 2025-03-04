namespace TicketSystemWeb.Models.KanbanBoard
{
    public class KanbanBoard
    {
        public int Id { get; set; }
        public string ProjectName { get; set; }
        public List<KanbanColumn> Columns { get; set; } = new();
    }

}
