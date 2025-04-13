using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketSystemDesktop.Models
{
    public class KanbanColumn
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public int Order { get; set; }

        public int KanbanBoardId { get; set; }
        public KanbanBoard KanbanBoard { get; set; }
    }
}

