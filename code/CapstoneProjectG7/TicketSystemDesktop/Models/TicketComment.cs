using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSystemDesktop.Models
{
    /// <summary>
    /// A comment left on a ticket.
    /// </summary>
    public class TicketComment
    {
        /// <summary>
        /// Gets or sets the comment id.
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the ticket id associated with the comment.
        /// </summary>
        [Required]
        public int TicketId { get; set; }

        /// <summary>
        /// Gets or sets the ticket associated with the comment.
        /// </summary>
        [ForeignKey("TicketId")]
        public Ticket Ticket { get; set; }

        /// <summary>
        /// Gets or sets the comment text.
        /// </summary>
        [Required]
        public string CommentText { get; set; }

        /// <summary>
        /// Gets or sets the author name.
        /// </summary>
        [Required]
        public string AuthorName { get; set; }

        /// <summary>
        /// Gets or sets the creation date of the comment.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
