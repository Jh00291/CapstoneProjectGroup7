using Microsoft.AspNetCore.Identity;

namespace TicketSystemWeb.Models.Employee
{
    /// <summary>
    /// employee class
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Identity.IdentityUser" />
    public class Employee : IdentityUser
    {
        /// <summary>
        /// Gets or sets the employee groups.
        /// </summary>
        /// <value>
        /// The employee groups.
        /// </value>
        public List<EmployeeGroup> EmployeeGroups { get; set; } = new();
    }

}
