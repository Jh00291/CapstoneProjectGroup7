using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TicketSystemWeb.Models;
using TicketSystemWeb.Models.Employee;
using TicketSystemWeb.Models.ProjectManagement.Group;
using TicketSystemWeb.Models.ProjectManagement.Project;

namespace TicketSystemWeb.Data
{
    public class TicketDBContext : IdentityDbContext<Employee>
    {
        public DbSet<Ticket> Tickets { get; set; }

        public DbSet<Group> Groups { get; set; }
        public DbSet<EmployeeGroup> EmployeeGroups { get; set; }

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

            modelBuilder.Entity<EmployeeGroup>()
                .HasKey(eg => new { eg.EmployeeId, eg.GroupId });

            modelBuilder.Entity<EmployeeGroup>()
                .HasOne(eg => eg.Employee)
                .WithMany(e => e.EmployeeGroups)
                .HasForeignKey(eg => eg.EmployeeId);

            modelBuilder.Entity<EmployeeGroup>()
                .HasOne(eg => eg.Group)
                .WithMany(g => g.EmployeeGroups)
                .HasForeignKey(eg => eg.GroupId);

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

            modelBuilder.Entity<Project>()
                .HasOne(p => p.ProjectManager)
                .WithMany()
                .HasForeignKey(p => p.ProjectManagerId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
