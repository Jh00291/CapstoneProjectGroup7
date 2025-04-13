using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketSystemDesktop.Models
{
    public class KanbanBoard
    {
        public int Id { get; set; }
        public string ProjectName { get; set; } = string.Empty;

        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public ICollection<KanbanColumn> Columns { get; set; }
    }
}

