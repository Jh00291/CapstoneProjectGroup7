using Microsoft.EntityFrameworkCore;
using System.Configuration;
using TicketSystemDesktop.Models;

namespace TicketSystemDesktop.Data
{
    public class TicketDBContext : DbContext
    {
        public DbSet<Ticket> Tickets { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                string connectionString = ConfigurationManager.ConnectionStrings["TicketDBConnection"].ConnectionString;
                optionsBuilder.UseSqlServer(connectionString);
            }
        }
    }
}
