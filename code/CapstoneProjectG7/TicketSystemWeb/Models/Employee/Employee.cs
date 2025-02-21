using Microsoft.AspNetCore.Identity;

namespace TicketSystemWeb.Models.Employee
{
    public class Employee : IdentityUser
    {
        public List<EmployeeGroup> EmployeeGroups { get; set; } = new();
    }

}
