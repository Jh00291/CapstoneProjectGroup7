using System.Collections.Generic;
using System.Linq;
using System.Windows;
using TicketSystemWeb.Data;
using TicketSystemWeb.Models;

namespace TicketSystemDesktop
{
    public partial class MainWindow : Window
    {
        private readonly TicketDBContext _context;

        public MainWindow()
        {
            InitializeComponent();
            _context = new TicketDBContext(new Microsoft.EntityFrameworkCore.DbContextOptions<TicketDBContext>());
            LoadTickets();
        }

        private void LoadTickets()
        {
            List<Ticket> tickets = _context.Tickets.ToList();
            TicketsDataGrid.ItemsSource = tickets;
        }
    }
}


