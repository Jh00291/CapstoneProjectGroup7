using Microsoft.EntityFrameworkCore;
using TicketSystemWeb.Models;


namespace TicketSystemWeb.Data
{
    public class TicketDBContext : DbContext
    {
        public TicketDBContext(DbContextOptions<TicketDBContext> options) : base(options) { }

        public DbSet<Ticket> Tickets { get; set; } // Example table
    }
}
