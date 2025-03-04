namespace TicketSystemWeb.Models.KanbanBoard
{
    public class KanbanColumn
    {
        public int Id { get; set; }
        public KanbanBoard KanbanBoard { get; set; }
        public string Name { get; set; }
        public int Order { get; set; }
        public List<Ticket> Tickets { get; set; } = new();
        public int KanbanBoardId { get; set; }
    }

}
