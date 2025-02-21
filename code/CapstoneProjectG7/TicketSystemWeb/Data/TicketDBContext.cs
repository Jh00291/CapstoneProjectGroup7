using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TicketSystemWeb.Models;

namespace TicketSystemWeb.Data
{
    public class TicketDBContext : IdentityDbContext<User>
    {
        public DbSet<Ticket> Tickets { get; set; }

        public DbSet<User> Users { get; set; }

        public TicketDBContext(DbContextOptions<TicketDBContext> options) : base(options) { }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=TicketSystemDb;Trusted_Connection=True;");
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        
    }
}
