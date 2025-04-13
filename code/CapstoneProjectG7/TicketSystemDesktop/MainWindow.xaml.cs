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
    public partial class MainWindow : Window
    {
        private readonly Employee _loggedInUser;

        public MainWindow(Employee user)
        {
            InitializeComponent();
            _loggedInUser = user;

            WelcomeText.Text = $"Welcome, {_loggedInUser.UserName}!";

            LoadTickets();

        }

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


        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var loginWindow = new LoginWindow();
            loginWindow.Show();
            this.Close();
        }

        private void TicketList_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            var listBox = sender as ListBox;
            var selectedTicket = listBox?.SelectedItem as Ticket;

            if (selectedTicket != null)
            {
                var detailsWindow = new TicketDetailsWindow(selectedTicket);
                detailsWindow.ShowDialog();
            }
        }

        private void MyTicketsOnlyCheckBox_Changed(object sender, RoutedEventArgs e)
        {
            LoadTickets();
        }

    }
}

