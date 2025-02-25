namespace TicketSystemWeb.Models.ProjectManagement.Project
{
    /// <summary>
    /// project class
    /// </summary>
    public class Project
    {
        /// <summary>
        /// Gets or sets the identifier.
        /// </summary>
        /// <value>
        /// The identifier.
        /// </value>
        public int Id { get; set; }
        /// <summary>
        /// Gets or sets the title.
        /// </summary>
        /// <value>
        /// The title.
        /// </value>
        public string Title { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the description.
        /// </summary>
        /// <value>
        /// The description.
        /// </value>
        public string Description { get; set; } = string.Empty;
        /// <summary>
        /// Gets or sets the project groups.
        /// </summary>
        /// <value>
        /// The project groups.
        /// </value>
        public List<ProjectGroup> ProjectGroups { get; set; } = new();
        /// <summary>
        /// Gets or sets the project manager identifier.
        /// </summary>
        /// <value>
        /// The project manager identifier.
        /// </value>
        public string? ProjectManagerId { get; set; }
        /// <summary>
        /// Gets or sets the project manager.
        /// </summary>
        /// <value>
        /// The project manager.
        /// </value>
        public Employee.Employee? ProjectManager { get; set; }
    }

}
