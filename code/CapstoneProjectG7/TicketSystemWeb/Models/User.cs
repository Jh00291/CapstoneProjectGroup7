using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace TicketSystemWeb.Models
{
    public class User : IdentityUser<int>
    {

        [Required]
        [StringLength(255)]
        public required string PasswordHash { get; set; }

        [Required]
        [StringLength(100)]
        public string Email { get; set; }

        [Required]
        public int RoleID { get; set; }

        [ForeignKey("RoleID")]
        public Role Role { get; set; }
    }

    public class Role : IdentityRole<int>
    {
        [Required]
        [StringLength(50)]
        public required string Name { get; set; }
    }
}
