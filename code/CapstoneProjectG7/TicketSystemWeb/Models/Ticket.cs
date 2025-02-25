using System.ComponentModel.DataAnnotations;

namespace TicketSystemWeb.Models
{
    /// <summary>
    /// the ticket class
    /// </summary>
    public class Ticket
    {
        /// <summary>
        /// Gets or sets the ticket identifier.
        /// </summary>
        /// <value>
        /// The ticket identifier.
        /// </value>
        [Key]
        public int TicketId { get; set; }

        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the status.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        [Required]
        public string Status { get; set; }

        /// <summary>
        /// Gets or sets the created at.
        /// </summary>
        /// <value>
        /// The created at.
        /// </value>
        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>
        /// Gets or sets the closed at.
        /// </summary>
        /// <value>
        /// The closed at.
        /// </value>
        public DateTime? ClosedAt { get; set; }

        /// <summary>
        /// Gets or sets the created by.
        /// </summary>
        /// <value>
        /// The created by.
        /// </value>
        [Required]
        public string CreatedBy { get; set; }
    }

}
