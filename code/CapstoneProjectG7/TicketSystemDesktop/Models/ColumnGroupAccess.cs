using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketSystemDesktop.Models
{
    public class ColumnGroupAccess
    {
        public int Id { get; set; }

        public int KanbanColumnId { get; set; }
        public KanbanColumn KanbanColumn { get; set; }

        public int GroupId { get; set; }
        public Group Group { get; set; }
    }
}
