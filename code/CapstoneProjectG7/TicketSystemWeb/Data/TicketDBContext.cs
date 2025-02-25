using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TicketSystemWeb.Models;
using TicketSystemWeb.Models.Employee;
using TicketSystemWeb.Models.ProjectManagement.Group;
using TicketSystemWeb.Models.ProjectManagement.Project;

namespace TicketSystemWeb.Data
{
    /// <summary>
    /// the ticket db context
    /// </summary>
    /// <seealso cref="Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityDbContext&lt;TicketSystemWeb.Models.Employee.Employee&gt;" />
    public class TicketDBContext : IdentityDbContext<Employee>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TicketDBContext"/> class.
        /// </summary>
        /// <param name="options">The options.</param>
        public TicketDBContext(DbContextOptions<TicketDBContext> options) : base(options) { }

        /// <summary>
        /// Gets or sets the tickets.
        /// </summary>
        /// <value>
        /// The tickets.
        /// </value>
        public DbSet<Ticket> Tickets { get; set; }
        /// <summary>
        /// Gets or sets the groups.
        /// </summary>
        /// <value>
        /// The groups.
        /// </value>
        public DbSet<Group> Groups { get; set; }
        /// <summary>
        /// Gets or sets the employee groups.
        /// </summary>
        /// <value>
        /// The employee groups.
        /// </value>
        public DbSet<EmployeeGroup> EmployeeGroups { get; set; }
        public DbSet<Project> Projects { get; set; }
        /// <summary>
        /// Gets or sets the project groups.
        /// </summary>
        /// <value>
        /// The project groups.
        /// </value>
        public DbSet<ProjectGroup> ProjectGroups { get; set; }

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
