using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TicketSystemDesktop.Data;
using TicketSystemDesktop.Models;

namespace TicketSystemDesktop
{
    /// <summary>
    /// the main window
    /// </summary>
    /// <seealso cref="System.Windows.Window" />
    /// <seealso cref="System.Windows.Markup.IComponentConnector" />
    public partial class MainWindow : Window
    {
        /// <summary>
        /// The logged in user
        /// </summary>
        private readonly Employee _loggedInUser;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        /// <param name="user">The user.</param>
        public MainWindow(Employee user)
        {
            InitializeComponent();
            _loggedInUser = user;

            WelcomeText.Text = $"Welcome, {_loggedInUser.UserName}!";

            LoadTickets();

        }

        /// <summary>
        /// Loads the tickets.
        /// </summary>
        private void LoadTickets()
        {
            using (var context = new TicketDBContext())
            {
                var showOnlyMyTickets = MyTicketsOnlyCheckBox.IsChecked == true;
                var showOnlyAvailableTickets = AvailableTicketsOnlyCheckBox.IsChecked == true;

                var ticketsQuery = context.Tickets
                    .Include(t => t.AssignedTo)
                    .Include(t => t.Project)
                    .AsQueryable();

                // Get the logged-in user's group IDs
                var userGroupIds = context.EmployeeGroups
                    .Where(eg => eg.EmployeeId == _loggedInUser.Id)
                    .Select(eg => eg.GroupId)
                    .ToList();

                // Get the columns that user's groups have access to
                var accessibleColumnNames = context.ColumnGroupAccesses
                    .Where(cga => userGroupIds.Contains(cga.GroupId))
                    .Select(cga => cga.KanbanColumn.Name)
                    .Distinct()
                    .ToList();

                if (showOnlyMyTickets)
                {
                    // Show only tickets assigned to this user
                    ticketsQuery = ticketsQuery.Where(t => t.AssignedToId == _loggedInUser.Id);
                }
                else if (showOnlyAvailableTickets)
                {
                    // Show only unassigned tickets that are in accessible columns
                    ticketsQuery = ticketsQuery.Where(t =>
                        t.AssignedToId == null &&
                        accessibleColumnNames.Contains(t.Status)
                    );
                }
                else
                {
                    // DEFAULT: when neither box is checked
                    // Show unassigned tickets (only ones accessible) + assigned-to-me tickets
                    ticketsQuery = ticketsQuery.Where(t =>
                        (t.AssignedToId == null && accessibleColumnNames.Contains(t.Status)) ||
                        (t.AssignedToId == _loggedInUser.Id)
                    );
                }

                UnifiedTicketsList.ItemsSource = ticketsQuery.ToList();
            }
        }



        /// <summary>
        /// Handles the Click event of the Logout control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        /// <summary>
        /// Handles the DoubleClick event of the TicketList control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseButtonEventArgs"/> instance containing the event data.</param>
        private void TicketList_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            try
            {
                var listBox = sender as ListBox;
                var selectedTicket = listBox?.SelectedItem as Ticket;

                if (selectedTicket != null)
                {
                    using (var context = new TicketDBContext())
                    {
                        var ticketWithProject = context.Tickets
                            .Include(t => t.AssignedTo)
                            .Include(t => t.Project)
                            .FirstOrDefault(t => t.TicketId == selectedTicket.TicketId);

                        if (ticketWithProject != null)
                        {
                            var detailsWindow = new TicketDetailsWindow(ticketWithProject, _loggedInUser);
                            detailsWindow.ShowDialog();

                            // After the TicketDetailsWindow closes, reload tickets
                            LoadTickets();
                        }
                        else
                        {
                            MessageBox.Show("Could not find full ticket in database.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred:\n" + ex.Message);
            }
        }


        /// <summary>
        /// Handles the Changed event of the MyTicketsOnlyCheckBox control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="RoutedEventArgs"/> instance containing the event data.</param>
        private void FilterCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            if (sender == MyTicketsOnlyCheckBox && MyTicketsOnlyCheckBox.IsChecked == true)
            {
                AvailableTicketsOnlyCheckBox.IsChecked = false;
            }
            else if (sender == AvailableTicketsOnlyCheckBox && AvailableTicketsOnlyCheckBox.IsChecked == true)
            {
                MyTicketsOnlyCheckBox.IsChecked = false;
            }

            LoadTickets();
        }

    }
}

