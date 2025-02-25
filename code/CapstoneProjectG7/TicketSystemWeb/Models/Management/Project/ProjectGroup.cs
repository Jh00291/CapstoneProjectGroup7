namespace TicketSystemWeb.Models.ProjectManagement.Project
{
    /// <summary>
    /// project group
    /// </summary>
    public class ProjectGroup
    {
        /// <summary>
        /// Gets or sets the project identifier.
        /// </summary>
        /// <value>
        /// The project identifier.
        /// </value>
        public int ProjectId { get; set; }
        /// <summary>
        /// Gets or sets the project.
        /// </summary>
        /// <value>
        /// The project.
        /// </value>
        public Project Project { get; set; } = null!;

        /// <summary>
        /// Gets or sets the group identifier.
        /// </summary>
        /// <value>
        /// The group identifier.
        /// </value>
        public int GroupId { get; set; }
        /// <summary>
        /// Gets or sets the group.
        /// </summary>
        /// <value>
        /// The group.
        /// </value>
        public Group.Group Group { get; set; } = null!;
    }

}
