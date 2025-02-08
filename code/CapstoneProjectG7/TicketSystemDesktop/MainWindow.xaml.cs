using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TicketSystemDesktop.Data;
using TicketSystemDesktop.Models;



namespace TicketSystemDesktop
{
    public partial class MainWindow : Window
    {

        public MainWindow()
        {
            InitializeComponent();
            LoadTickets();
            
        }

        private void LoadTickets()
        {
            using (var context = new TicketDBContext())
            {
                List<Ticket> tickets = context.Tickets.ToList();
                TicketDataGrid.ItemsSource = tickets;
            }
        }
    }
}


