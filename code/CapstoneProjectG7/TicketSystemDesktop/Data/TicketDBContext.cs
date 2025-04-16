using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using TicketSystemDesktop.Models;

namespace TicketSystemDesktop.Data
{
    public class TicketDBContext : IdentityDbContext<Employee>
    {
        public DbSet<Ticket> Tickets { get; set; }

        public DbSet<Employee> Employees { get; set; }

        public TicketDBContext() { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=TicketSystemDb;Trusted_Connection=True;");
            }
        }
    }
}
