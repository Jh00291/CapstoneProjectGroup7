using System.ComponentModel.DataAnnotations;

namespace TicketSystemWeb.Models
{
    /// <summary>
    /// login view model
    /// </summary>
    public class LoginViewModel
    {
        /// <summary>
        /// Gets or sets the username.
        /// </summary>
        /// <value>
        /// The username.
        /// </value>
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
        public required string Username { get; set; }

        /// <summary>
        /// Gets or sets the password.
        /// </summary>
        /// <value>
        /// The password.
        /// </value>
        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        [StringLength(255, ErrorMessage = "Password cannot exceed 255 characters.")]
        public required string Password { get; set; }
    }
}
