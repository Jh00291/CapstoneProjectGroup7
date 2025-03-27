using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TicketSystemWeb.Models.Employee;
using TicketSystemWeb.Models.KanbanBoard;
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
        public virtual DbSet<Group> Groups { get; set; }
        /// <summary>
        /// Gets or sets the employee groups.
        /// </summary>
        /// <value>
        /// The employee groups.
        /// </value>
        public virtual DbSet<EmployeeGroup> EmployeeGroups { get; set; }
        /// <summary>
        /// Gets or sets the projects.
        /// </summary>
        /// <value>
        /// The projects.
        /// </value>
        public DbSet<Project> Projects { get; set; }
        /// <summary>
        /// Gets or sets the project groups.
        /// </summary>
        /// <value>
        /// The project groups.
        /// </value>
        public DbSet<ProjectGroup> ProjectGroups { get; set; }
        /// <summary>
        /// Gets or sets the kanban boards.
        /// </summary>
        /// <value>
        /// The kanban boards.
        /// </value>
        public DbSet<KanbanBoard> KanbanBoards { get; set; }
        /// <summary>
        /// Gets or sets the kanban columns.
        /// </summary>
        /// <value>
        /// The kanban columns.
        /// </value>
        public DbSet<KanbanColumn> KanbanColumns { get; set; }
        /// <summary>
        /// Gets or sets the ticket histories.
        /// </summary>
        /// <value>
        /// The ticket histories.
        /// </value>
        public DbSet<TicketHistory> TicketHistories { get; set; }
        /// <summary>
        /// Gets or sets the ticket comments.
        /// </summary>
        /// <value>
        /// The ticket comments.
        /// </value>
        public DbSet<TicketComment> TicketComments { get; set; }

        /// <summary>
        /// Gets or sets the Employees.
        /// </summary>
        /// <value>
        /// The employees.
        /// </value>
        public DbSet<Employee> Employees { get; set; }



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
                .HasOne(p => p.KanbanBoard)
                .WithOne(b => b.Project)
                .HasForeignKey<KanbanBoard>(b => b.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Project>()
                .HasMany(p => p.Tickets)
                .WithOne(t => t.Project)
                .HasForeignKey(t => t.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<KanbanBoard>()
                .HasMany(b => b.Columns)
                .WithOne(c => c.KanbanBoard)
                .HasForeignKey(c => c.KanbanBoardId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Project>()
                .HasOne(p => p.ProjectManager)
                .WithMany()
                .HasForeignKey(p => p.ProjectManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Group>()
                .HasOne(g => g.Manager)
                .WithMany()
                .HasForeignKey(g => g.ManagerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Ticket>()
                .HasOne(t => t.AssignedTo)
                .WithMany()
                .HasForeignKey(t => t.AssignedToId)
                .OnDelete(DeleteBehavior.SetNull);

        }
    }
}
