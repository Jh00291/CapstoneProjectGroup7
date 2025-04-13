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
        private readonly Ticket _ticket;

        public TicketDetailsWindow(Ticket ticket)
        {
            InitializeComponent();
            _ticket = ticket;
            DataContext = _ticket;
            LoadStages();
        }

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
    }

}
