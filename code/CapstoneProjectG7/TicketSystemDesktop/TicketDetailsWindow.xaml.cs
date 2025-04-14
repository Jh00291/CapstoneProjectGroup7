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
            DataContext = _ticket;
            LoadStages();
            _currentUser = currentUser;
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
            var selected = StageDropdown.SelectedValue as string;
            if (!string.IsNullOrEmpty(selected))
            {
                _ticket.Status = selected;

                using var context = new TicketDBContext();
                context.Attach(_ticket);
                context.Entry(_ticket).Property(t => t.Status).IsModified = true;
                context.SaveChanges();
            }
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
    }

}
