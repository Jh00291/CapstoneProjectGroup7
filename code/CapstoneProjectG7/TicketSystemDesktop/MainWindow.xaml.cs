using System.Collections.Generic;
using System.Linq;
using System.Windows;
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
                var assignedTickets = context.Tickets
                    .Where(t => t.AssignedToId == _loggedInUser.Id)
                    .ToList();

                var availableTickets = context.Tickets
                    .Where(t => t.AssignedToId == null)
                    .ToList();

                AssignedTicketsList.ItemsSource = assignedTickets;
                AvailableTicketsList.ItemsSource = availableTickets;
            }
        }


    }
}

