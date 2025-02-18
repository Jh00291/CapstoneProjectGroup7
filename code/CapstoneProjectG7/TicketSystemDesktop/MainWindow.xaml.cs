using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
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

        private void AddTicket_Click(object sender, RoutedEventArgs e)
        {
            string title = TitleTextBox.Text.Trim();
            string description = DescriptionTextBox.Text.Trim();
            string status = (StatusComboBox.SelectedItem as ComboBoxItem)?.Content.ToString();
            string createdBy = CreatedByTextBox.Text.Trim();

            if (string.IsNullOrEmpty(title) || string.IsNullOrEmpty(description) ||
                string.IsNullOrEmpty(status) || string.IsNullOrEmpty(createdBy))
            {
                MessageBox.Show("All fields are required.", "Validation Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            using (var context = new TicketDBContext())
            {
                Ticket newTicket = new Ticket
                {
                    Title = title,
                    Description = description,
                    Status = status,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = createdBy
                };

                context.Tickets.Add(newTicket);
                context.SaveChanges();
            }

            MessageBox.Show("Ticket added successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            LoadTickets(); // Refresh the list after adding
        }
    }
}
