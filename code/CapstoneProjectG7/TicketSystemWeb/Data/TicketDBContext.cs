using Microsoft.EntityFrameworkCore;
using System;
using TicketSystemWeb.Models;

namespace TicketSystemWeb.Data
{
    public class TicketDBContext : DbContext
    {
        public TicketDBContext(DbContextOptions<TicketDBContext> options) : base(options) { }

        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Ticket>().HasData(
                new Ticket
                {
                    TicketId = 1,
                    Title = "First Ticket",
                    Description = "This is a test ticket.",
                    Status = "Open",
                    CreatedAt = new DateTime(2024, 2, 4, 12, 5, 0, DateTimeKind.Utc),
                    CreatedBy = "Admin"
                },
                new Ticket
                {
                    TicketId = 2,
                    Title = "Second Ticket",
                    Description = "Another test ticket.",
                    Status = "In Progress",
                    CreatedAt = new DateTime(2024, 2, 4, 12, 0, 0, DateTimeKind.Utc),
                    CreatedBy = "User123"
                }
            );
        }
    }
}
