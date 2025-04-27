using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Configuration;
using TicketSystemDesktop.Models;

namespace TicketSystemDesktop.Data
{
    public class TicketDBContext : IdentityDbContext<Employee>
    {
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Group> Groups { get; set; }
        public DbSet<EmployeeGroup> EmployeeGroups { get; set; }
        public DbSet<ProjectGroup> ProjectGroups { get; set; }
        public DbSet<ColumnGroupAccess> ColumnGroupAccesses { get; set; }
        public DbSet<KanbanBoard> KanbanBoards { get; set; }
        public DbSet<KanbanColumn> KanbanColumns { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<Ticket> Tickets { get; set; }


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

            // Ticket - Project
            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.Project)
                .WithMany(p => p.Tickets)
                .HasForeignKey(t => t.ProjectId);

            // KanbanBoard - Columns
            modelBuilder.Entity<KanbanBoard>()
                .HasMany(b => b.Columns)
                .WithOne(c => c.KanbanBoard)
                .HasForeignKey(c => c.KanbanBoardId);

            // Project - KanbanBoard
            modelBuilder.Entity<Project>()
                .HasOne(p => p.KanbanBoard)
                .WithOne(b => b.Project)
                .HasForeignKey<KanbanBoard>(b => b.ProjectId);

            // EmployeeGroup composite key and relations
            modelBuilder.Entity<EmployeeGroup>()
                .HasKey(eg => new { eg.EmployeeId, eg.GroupId });

            modelBuilder.Entity<EmployeeGroup>()
                .HasOne(eg => eg.Employee)
                .WithMany()
                .HasForeignKey(eg => eg.EmployeeId);

            modelBuilder.Entity<EmployeeGroup>()
                .HasOne(eg => eg.Group)
                .WithMany(g => g.EmployeeGroups)
                .HasForeignKey(eg => eg.GroupId);

            // ProjectGroup composite key and relations
            modelBuilder.Entity<ProjectGroup>()
                .HasKey(pg => new { pg.ProjectId, pg.GroupId });

            modelBuilder.Entity<ProjectGroup>()
                .HasOne(pg => pg.Project)
                .WithMany(p => p.ProjectGroups)
                .HasForeignKey(pg => pg.ProjectId);

            modelBuilder.Entity<ProjectGroup>()
                .HasOne(pg => pg.Group)
                .WithMany(g => g.ProjectGroups)
                .HasForeignKey(pg => pg.GroupId);

            // ColumnGroupAccess relationships
            modelBuilder.Entity<ColumnGroupAccess>()
                .HasOne(cga => cga.KanbanColumn)
                .WithMany(kc => kc.GroupAccess)
                .HasForeignKey(cga => cga.KanbanColumnId);

            modelBuilder.Entity<ColumnGroupAccess>()
                .HasOne(cga => cga.Group)
                .WithMany()
                .HasForeignKey(cga => cga.GroupId);

        }
    }
}
