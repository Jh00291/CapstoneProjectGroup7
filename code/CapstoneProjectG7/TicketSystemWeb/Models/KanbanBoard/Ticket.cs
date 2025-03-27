using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TicketSystemWeb.Models.ProjectManagement.Project;

namespace TicketSystemWeb.Models.KanbanBoard
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

        /// <summary>
        /// Gets or sets the project identifier.
        /// </summary>
        /// <value>
        /// The project identifier.
        /// </value>
        public int ProjectId { get; set; }

        /// <summary>
        /// Gets or sets the project.
        /// </summary>
        /// <value>
        /// The project.
        /// </value>
        public Project Project { get; set; }

        /// <summary>
        /// Gets or sets the history.
        /// </summary>
        /// <value>
        /// The history.
        /// </value>
        public ICollection<TicketHistory> History { get; set; } = new List<TicketHistory>();

        /// <summary>
        /// Gets or sets the comments.
        /// </summary>
        /// <value>
        /// The comments.
        /// </value>
        public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();

        /// <summary>
        /// Gets or sets the Id of employee a ticket is assigned to.
        /// </summary>
        /// <value>
        /// The assigned to Employee Id
        /// </value>
        public string? AssignedToId { get; set; }

        /// <summary>
        /// Gets or sets the assigned employee of a ticket.
        /// </summary>
        /// <value>
        /// The employee
        /// </value>
        public Employee.Employee? AssignedTo { get; set; }

    }

}
