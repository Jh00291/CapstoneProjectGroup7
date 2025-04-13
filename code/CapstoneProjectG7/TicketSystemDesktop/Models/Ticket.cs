using System;
using System.ComponentModel.DataAnnotations;

namespace TicketSystemDesktop.Models
{
    public class Ticket
    {
        [Key]
        public int TicketId { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        [StringLength(500)]
        public string Description { get; set; }

        [Required]
        public string Status { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ClosedAt { get; set; }

        [Required]
        public string CreatedBy { get; set; }

        public int ProjectId { get; set; }

        public Project? Project { get; set; }

        public string? AssignedToId { get; set; }

        public Employee? AssignedTo { get; set; }

        // Optional: include if you implement these in desktop app
        // public ICollection<TicketHistory> History { get; set; } = new List<TicketHistory>();
        // public ICollection<TicketComment> Comments { get; set; } = new List<TicketComment>();
    }
}