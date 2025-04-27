using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketSystemDesktop.Models
{
    public class EmployeeGroup
    {
        public string EmployeeId { get; set; }
        public Employee Employee { get; set; }

        public int GroupId { get; set; }
        public Group Group { get; set; }
    }
}
