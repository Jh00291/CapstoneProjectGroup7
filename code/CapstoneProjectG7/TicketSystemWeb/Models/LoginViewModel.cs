using System.ComponentModel.DataAnnotations;

namespace TicketSystemWeb.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
        public required string Username { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(255, ErrorMessage = "Password cannot exceed 255 characters.")]
        public required string Password { get; set; }
    }
}
