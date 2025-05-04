using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using TicketSystemDesktop.Data;
using TicketSystemDesktop.Models;

namespace TicketSystemDesktop
{
    /// <summary>
    /// Interaction logic for TicketDetailsWindow.xaml
    /// </summary>
    public partial class TicketDetailsWindow : Window
    {
        /// <summary>
        /// The ticket
        /// </summary>
        private readonly Ticket _ticket;
        /// <summary>
        /// The current user
        /// </summary>
        private readonly Employee _currentUser;

        /// <summary>
        /// Initializes a new instance of the <see cref="TicketDetailsWindow"/> class.
        /// </summary>
        /// <param name="ticket">The ticket.</param>
        /// <param name="currentUser">The current user.</param>
        public TicketDetailsWindow(Ticket ticket, Employee currentUser)
        {
            InitializeComponent();
            _ticket = ticket;
            _currentUser = currentUser;
            DataContext = _ticket;
            LoadStages();
            LoadComments();
        }

        /// <summary>
        /// Loads the stages.
        /// </summary>
        private void LoadStages()
        {
            using var context = new TicketDBContext();

            var columns = context.KanbanColumns
                .Include(c => c.KanbanBoard)
                .Where(c => c.KanbanBoard.ProjectId == _ticket.ProjectId)
                .OrderBy(c => c.Order)
                .ToList();

            StageDropdown.ItemsSource = columns;
            StageDropdown.SelectedValue = _ticket.Status;
        }

        /// <summary>
        /// Handles the SelectionChanged event of the StageDropdown control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="SelectionChangedEventArgs"/> instance containing the event data.</param>
        private void StageDropdown_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (StageDropdown.SelectedItem == null || StageDropdown.SelectedItem is not KanbanColumn selectedColumn)
            {
                return;
            }

            using var context = new TicketDBContext();

            var ticket = context.Tickets.FirstOrDefault(t => t.TicketId == _ticket.TicketId);
            if (ticket == null)
            {
                MessageBox.Show("Error: Ticket not found.");
                return;
            }

            if (_currentUser == null)
            {
                MessageBox.Show("Error: Current user not set.");
                return;
            }

            var userId = _currentUser.Id;

            var allowedGroupIds = context.ColumnGroupAccesses
                .Where(cga => cga.KanbanColumnId == selectedColumn.Id)
                .Select(cga => cga.GroupId)
                .ToList();

            var userGroupIds = context.EmployeeGroups
                .Where(eg => eg.EmployeeId == userId)
                .Select(eg => eg.GroupId)
                .ToList();

            bool hasAccess = allowedGroupIds.Intersect(userGroupIds).Any();

            if (!hasAccess)
            {
                var result = MessageBox.Show(
                    "You will not have access to this ticket if you move it into the selected stage. " +
                    "You will be unassigned and the window will close. Continue?",
                    "Warning",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes)
                {
                    StageDropdown.SelectedValue = _ticket.Status;
                    return;
                }
                else
                {
                    ticket.Status = selectedColumn.Name;
                    ticket.AssignedToId = null;
                    context.SaveChanges();

                    // Update the local ticket reference
                    _ticket.Status = selectedColumn.Name;
                    _ticket.AssignedTo = null;

                    // Optionally refresh the UI first
                    DataContext = null;
                    DataContext = _ticket;

                    // Close the window since user will lose access
                    this.Close();
                    return;
                }
            }

            // Normal update if access OK
            ticket.Status = selectedColumn.Name;
            context.SaveChanges();

            _ticket.Status = selectedColumn.Name;
            DataContext = null;
            DataContext = _ticket;
        }






        /// <summary>
        /// Handles the Click event of the AssignToMe control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void AssignToMe_Click(object sender, RoutedEventArgs e)
        {
            using var context = new TicketDBContext();
            var ticket = context.Tickets.FirstOrDefault(t => t.TicketId == _ticket.TicketId);
            if (ticket != null)
            {
                ticket.AssignedToId = _currentUser.Id;
                context.SaveChanges();
                _ticket.AssignedTo = _currentUser;
                DataContext = null;
                DataContext = _ticket;
            }
        }

        /// <summary>
        /// Handles the Click event of the UnassignMe control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void UnassignMe_Click(object sender, RoutedEventArgs e)
        {
            using var context = new TicketDBContext();
            var ticket = context.Tickets.FirstOrDefault(t => t.TicketId == _ticket.TicketId);
            if (ticket != null && ticket.AssignedToId == _currentUser.Id)
            {
                ticket.AssignedToId = null;
                context.SaveChanges();
                _ticket.AssignedTo = null;
                DataContext = null;
                DataContext = _ticket;
            }
        }

        private void LoadComments()
        {
            using var context = new TicketDBContext();
            var comments = context.TicketComments
                .Where(c => c.TicketId == _ticket.TicketId)
                .OrderByDescending(c => c.CreatedAt)
                .ToList();

            CommentsListBox.ItemsSource = comments;
        }

        private void AddComment_Click(object sender, RoutedEventArgs e)
        {
            var commentText = NewCommentTextBox.Text.Trim();

            if (string.IsNullOrWhiteSpace(commentText))
            {
                MessageBox.Show("Please enter a comment before submitting.");
                return;
            }

            using var context = new TicketDBContext();
            var comment = new TicketComment
            {
                TicketId = _ticket.TicketId,
                CommentText = commentText,
                AuthorName = _currentUser.UserName,
                CreatedAt = DateTime.UtcNow
            };

            context.TicketComments.Add(comment);
            context.SaveChanges();

            NewCommentTextBox.Clear();
            LoadComments();
        }
    }

}
