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

        public DbSet<Project> Projects { get; set; }
        public DbSet<KanbanBoard> KanbanBoards { get; set; }
        public DbSet<KanbanColumn> KanbanColumns { get; set; }


        public TicketDBContext() { }

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

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Tickets)
                .HasForeignKey(t => t.ProjectId);

            modelBuilder.Entity<KanbanBoard>()
                .HasMany(b => b.Columns)
                .WithOne(c => c.KanbanBoard)
                .HasForeignKey(c => c.KanbanBoardId);

            modelBuilder.Entity<Project>()
                .HasOne(p => p.KanbanBoard)
                .WithOne(b => b.Project)
                .HasForeignKey<KanbanBoard>(b => b.ProjectId);

        }
    }
}
