using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TicketSystemDesktop.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // ManagerId is optional
        public string? ManagerId { get; set; }

        // Navigation properties
        public ICollection<EmployeeGroup> EmployeeGroups { get; set; } = new List<EmployeeGroup>();
        public ICollection<ProjectGroup> ProjectGroups { get; set; } = new List<ProjectGroup>();
        public ICollection<ColumnGroupAccess> ColumnGroupAccesses { get; set; } = new List<ColumnGroupAccess>();
    }
}
