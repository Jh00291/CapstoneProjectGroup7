using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketSystemDesktop.Models
{
    public class ProjectGroup
    {
        public int ProjectId { get; set; }
        public Project Project { get; set; }

        public int GroupId { get; set; }
        public Group Group { get; set; }

        public bool IsApproved { get; set; }
    }
}

