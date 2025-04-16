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

                var tickets = context.Tickets
                    .Include(t => t.AssignedTo)
                    .Where(t => !showOnlyMyTickets || t.AssignedToId == _loggedInUser.Id)
                    .ToList();

                UnifiedTicketsList.ItemsSource = tickets;
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
        private void MyTicketsOnlyCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            LoadTickets();
        }

    }
}

