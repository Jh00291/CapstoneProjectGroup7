using System.ComponentModel.DataAnnotations;

namespace TicketSystemWeb.Models
{
    public class Ticket
    {
        [Key]  // Marks this as the Primary Key
        public int TicketId { get; set; }

        [Required]  // Ensures Title is not null
        [StringLength(100)]  // Limits Title to 100 characters
        public string Title { get; set; }

        [Required]
        [StringLength(500)]  // Description can be up to 500 characters
        public string Description { get; set; }

        [Required]
        public string Status { get; set; } // Example: "Open", "In Progress", "Closed"

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;  // Default to current time

        public DateTime? ClosedAt { get; set; }  // Nullable if ticket is not closed yet

        [Required]
        public string CreatedBy { get; set; }  // The user who created the ticket
    }

}
