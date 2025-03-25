using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TicketSystemWeb.Models.KanbanBoard
{
    /// <summary>
    /// the ticket comment
    /// </summary>
    public class TicketComment
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Gets or sets the ticket identifier.
        /// </summary>
        /// <value>
        /// The ticket identifier.
        /// </value>
        [Required]
        public int TicketId { get; set; }

        /// <summary>
        /// Gets or sets the ticket.
        /// </summary>
        /// <value>
        /// The ticket.
        /// </value>
        [ForeignKey("TicketId")]
        public Ticket Ticket { get; set; }

        /// <summary>
        /// Gets or sets the comment text.
        /// </summary>
        /// <value>
        /// The comment text.
        /// </value>
        [Required]
        public string CommentText { get; set; }

        /// <summary>
        /// Gets or sets the name of the author.
        /// </summary>
        /// <value>
        /// The name of the author.
        /// </value>
        public string AuthorName { get; set; }

        /// <summary>
        /// Gets or sets the created at.
        /// </summary>
        /// <value>
        /// The created at.
        /// </value>
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
