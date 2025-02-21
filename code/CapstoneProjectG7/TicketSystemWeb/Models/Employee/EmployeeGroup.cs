using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using TicketSystemWeb.Models.ProjectManagement.Group;

namespace TicketSystemWeb.Models.Employee
{
    public class EmployeeGroup
    {
        [Key, Column(Order = 0)]
        public string EmployeeId { get; set; }
        public Employee Employee { get; set; } = null!;

        [Key, Column(Order = 1)]
        public int GroupId { get; set; }
        public Group Group { get; set; } = null!;
    }

}
